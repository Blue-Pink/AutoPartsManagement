<script setup lang="ts">
import { ref, watch } from 'vue'
import { ElMessage, type FormInstance } from 'element-plus'
import type { OutboundItem, Part } from '@/interfaces/Entities'
import { _initialOutboundItem, createDefaultEntity } from '@/utils/initialEntity'
import UsualEntityService from '@/services/UsualEntityService'
import { ConstDictionary } from '@/utils/const-dictionary'
import { NumberToFixed } from '@/utils/converter'

const props = defineProps<{
  modelValue: boolean
  item: OutboundItem | null
  orderId: string | null
}>()
const emit = defineEmits(['update:modelValue', 'saved'])
const visible = ref(props.modelValue)
const outboundItem = ref<OutboundItem>(
  createDefaultEntity<OutboundItem>({ ..._initialOutboundItem }),
)
const parts = ref<Part[]>([])
const formRef = ref<FormInstance>()
const rules = {
  partId: [{ required: true, message: '请选择配件', trigger: 'blur' }],
  quantity: [{ required: true, message: '请输入数量', trigger: 'blur' }],
  price: [{ required: true, message: '请输入单价', trigger: 'blur' }],
}

const loadParts = async () => {
  try {
    const p = await UsualEntityService.GetDataSet<Part>('Part', 0, 0, 'stockpiles', true)
    parts.value = p.dataList || []
  } catch (e) {
    console.error(e)
  }
}

const close = () => {
  formRef.value?.clearValidate()
  emit('update:modelValue', false)
}

const handlePartChange = (partId: string) => {
  const selectedPart = parts.value.find((p) => p.id === partId)
  if (selectedPart) {
    outboundItem.value.price = selectedPart.sellingPrice || 0
    handlePriceChange(outboundItem.value.price || 0)
  }
}

const handleQuantityChange = (quantity: number) => {
  if (outboundItem.value.price) {
    outboundItem.value.totalAmount = NumberToFixed(
      quantity * (outboundItem.value.price || 0) || 0.01,
    )
  }
}

const handlePriceChange = (price: number) => {
  if (outboundItem.value.quantity) {
    outboundItem.value.totalAmount = NumberToFixed(
      (outboundItem.value.quantity || 0) * (price || 0) || 0.01,
    )
  }
}

const handleSave = async () => {
  try {
    await formRef.value?.validate?.()
    outboundItem.value.outboundOrderId = props.orderId || outboundItem.value.outboundOrderId
    outboundItem.value.totalAmount = NumberToFixed(
      (outboundItem.value.quantity || 0) * (outboundItem.value.price || 0) || 0.01,
    )
    await UsualEntityService.Edit('OutboundItem', outboundItem.value)
    ElMessage.success('保存成功')
    emit('saved')
    close()
  } catch (e) {
    console.error(e)
  }
}

watch(
  () => props.modelValue,
  (v) => {
    if (v)
      loadParts().then(() => {
        visible.value = v
      })
    else {
      visible.value = v
    }
  },
)

watch(visible, (v) => emit('update:modelValue', v))

watch(
  () => props.item,
  (i) => {
    if (i && i.id && i.id !== ConstDictionary.EMPTY_GUID) {
      UsualEntityService.Get<OutboundItem>('OutboundItem', i.id).then((res) => {
        if (res && res.data) outboundItem.value = res.data
      })
    } else {
      outboundItem.value = createDefaultEntity<OutboundItem>({ ..._initialOutboundItem })
    }
  },
)
</script>

<template>
  <el-dialog v-model="visible" title="明细编辑" width="36vw" @close="close">
    <el-form :model="outboundItem" :rules="rules" ref="formRef" label-width="100px">
      <el-form-item label="配件" prop="partId">
        <el-select v-model="outboundItem.partId" placeholder="选择配件" @change="handlePartChange">
          <el-option
            v-for="p in parts"
            :key="p.id"
            :label="`[${p.partName}] - [${p.model}] 
            - [¥${p.sellingPrice}/${p.unit?.name}] 
            - [当前${p.stockpiles}${p.unit?.name}(水位线${p.minStock}至${p.maxStock})]`"
            :value="p.id"
          />
        </el-select>
      </el-form-item>
      <el-form-item label="数量" prop="quantity">
        <el-input-number v-model="outboundItem.quantity" :min="1" @change="handleQuantityChange" />
      </el-form-item>
      <el-form-item label="单价" prop="price">
        <el-input-number
          v-model="outboundItem.price"
          :min="0.01"
          :step="0.01"
          @change="handlePriceChange"
        />
      </el-form-item>
      <el-form-item label="合计" prop="totalAmount">
        <el-input-number v-model="outboundItem.totalAmount" :min="0.01" :step="0.01" disabled />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="close">取消</el-button>
      <el-button type="primary" @click="handleSave">保存</el-button>
    </template>
  </el-dialog>
</template>
