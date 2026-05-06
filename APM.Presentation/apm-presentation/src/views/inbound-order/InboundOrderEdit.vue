<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { InboundOrder, Supplier } from '@/interfaces/Entities'
import { ElMessage, type FormInstance } from 'element-plus'
import InboundItem from '@/components/inbound-order/InboundItem.vue'
import UsualEntityService from '@/services/UsualEntityService'
import { _initialInboundOrder } from '@/utils/initialEntity'
import UserService from '@/services/UserService'
import { ConstDictionary } from '@/utils/const-dictionary'

const route = useRoute()
const router = useRouter()
const id = ref<string | undefined>(route.params.id as string | undefined)
const emit = defineEmits(['saved'])

const order = ref<InboundOrder>({ ..._initialInboundOrder })
const suppliers = ref<Supplier[]>([])
const formRef = ref<FormInstance>()
const rules = ref({
  supplierId: [{ required: true, message: '请选择供应商', trigger: 'blur' }],
})

const loadOptions = async () => {
  try {
    const s = await UsualEntityService.GetDataSet<Supplier>('Supplier', 0)
    suppliers.value = s.dataList || []
  } catch (e) {
    console.error(e)
  }
}

const load = async () => {
  if (id.value && id.value !== ConstDictionary.EMPTY_GUID) {
    try {
      const res = await UsualEntityService.Get<InboundOrder>('InboundOrder', id.value)
      if (res.stateCode && res.data) order.value = res.data as InboundOrder
    } catch (e) {
      console.error(e)
    }
  } else {
    try {
      const orderNoRes = await UsualEntityService.AutoNumber('InboundOrder', 'RKD')
      const UserRes = await UserService.GetCurrentUser()
      if (UserRes.data) {
        order.value.operatorUserId = UserRes.data.id
        order.value.operatorUser = UserRes.data
      }
      if (orderNoRes.data) order.value.orderNo = orderNoRes.data
    } catch (e) {
      console.error(e)
    }
  }
}

const handleSave = async () => {
  try {
    await formRef.value?.validate?.()
    UsualEntityService.Edit<InboundOrder>('InboundOrder', order.value)
      .then((res) => {
        if (res.data && !id.value) {
          router.replace(`/inbound-order/edit/${res.data.id}`)
        } else {
          load()
          ElMessage.success('保存成功')
        }
      })
      .catch((e) => {
        console.error(e)
      })
  } catch (e) {
    console.error(e)
  }
}

const handleSaved = async () => {
  emit('saved')
  await load()
}

onMounted(async () => {
  await loadOptions()
  await load()
})

watch(
  () => route.params.id,
  async () => {
    id.value = route.params.id as string | undefined
    await load()
  },
)
</script>

<template>
  <div class="apm-editor-container">
    <div class="apm-container">
      <div class="title">入库单 {{ id ? order.orderNo : '新建' }}</div>
      <el-form :model="order" ref="formRef" label-width="100px" :rules="rules">
        <el-row :gutter="16">
          <el-col :span="6">
            <el-form-item label="订单号" prop="orderNo">
              <el-input v-model="order.orderNo" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="供应商" prop="supplierId">
              <el-select v-model="order.supplierId" placeholder="选择供应商">
                <el-option v-for="s in suppliers" :key="s.id" :label="s.name" :value="s.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="总金额" prop="totalAmount">
              <el-input-number v-model="order.totalAmount" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="经办人" prop="operatorUserId">
              <el-input disabled :value="order.operatorUser?.realname" />
            </el-form-item>
          </el-col>

          <el-col :span="24">
            <el-form-item label="备注">
              <el-input type="textarea" v-model="order.remark" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>

      <div class="button-group">
        <el-button type="primary" @click="handleSave">保存</el-button>
        <el-button @click="router.back">返回</el-button>
      </div>
    </div>
    <div class="apm-container" v-if="id">
      <InboundItem :orderId="order.id" @saved="handleSaved" />
    </div>
  </div>
</template>
