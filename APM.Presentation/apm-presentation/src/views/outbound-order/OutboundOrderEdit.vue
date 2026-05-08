<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { OutboundOrder, Customer } from '@/interfaces/Entities'
import { dayjs, ElMessage, type FormInstance } from 'element-plus'
import OutboundItem from '@/components/outbound-order/OutboundItem.vue'
import UsualEntityService from '@/services/UsualEntityService'
import UserService from '@/services/UserService'
import { ConstDictionary } from '@/utils/const-dictionary'
import { _initialOutboundOrder } from '@/utils/initialEntity'

const route = useRoute()
const router = useRouter()
const id = ref<string | undefined>(route.params.id as string | undefined)
const emit = defineEmits(['saved'])

const order = ref<OutboundOrder>({ ..._initialOutboundOrder })
const customers = ref<Customer[]>([])
const formRef = ref<FormInstance>()
const rules = ref({
  customerId: [{ required: true, message: '请选择客户', trigger: 'blur' }],
  outboundDate: [{ required: true, message: '请选择出库时间', trigger: 'blur' }],
})

const loadOptions = async () => {
  try {
    const c = await UsualEntityService.GetDataSet<Customer>('Customer', 0)
    customers.value = c.dataList || []
  } catch (e) {
    console.error(e)
  }
}

const load = async () => {
  if (id.value && id.value !== ConstDictionary.EMPTY_GUID) {
    try {
      const res = await UsualEntityService.Get<OutboundOrder>('OutboundOrder', id.value)
      if (res.stateCode && res.data) order.value = res.data as OutboundOrder
    } catch (e) {
      console.error(e)
    }
  } else {
    try {
      const orderNoRes = await UsualEntityService.AutoNumber('OutboundOrder', 'CKD')
      const userRes = await UserService.GetCurrentUser()
      if (userRes.data) {
        order.value.operatorUserId = userRes.data.id
      }
      if (orderNoRes.data) order.value.orderNo = orderNoRes.data
      if (!order.value.outboundDate) order.value.outboundDate = ConstDictionary.CURRENT_DATETIME
    } catch (e) {
      console.error(e)
    }
  }
}

const handleSave = async () => {
  try {
    await formRef.value?.validate?.()
    await UsualEntityService.Edit<OutboundOrder>('OutboundOrder', order.value)
      .then((res) => {
        if (res.data && !id.value) {
          router.replace(`/outbound-order/edit/${res.data.id}`)
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
      <div class="title">出库单 {{ id ? order.orderNo : '新建' }}</div>
      <el-form :model="order" ref="formRef" label-width="100px" :rules="rules">
        <el-row :gutter="16">
          <el-col :span="6">
            <el-form-item label="订单号" prop="orderNo">
              <el-input v-model="order.orderNo" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="客户" prop="customerId">
              <el-select v-model="order.customerId" placeholder="选择客户">
                <el-option v-for="c in customers" :key="c.id" :label="c.name" :value="c.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="出库时间" prop="outboundDate">
              <el-date-picker
                v-model="order.outboundDate"
                type="datetime"
                placeholder="选择出库时间"
                format="YYYY-MM-DD HH:mm:ss"
                value-format="YYYY-MM-DD HH:mm:ss"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="总金额" prop="totalAmount">
              <el-input-number v-model="order.totalAmount" disabled />
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
      <OutboundItem :orderId="order.id" @saved="handleSaved" />
    </div>
  </div>
</template>
