<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import InboundOrderService from '@/services/InboundOrderService'
import SupplierService from '@/services/SupplierService'
import type { InboundOrder, Supplier } from '@/interfaces/DTOEntities'
import { ElMessage, type FormInstance } from 'element-plus'
import InboundItem from '@/components/inbound-order/InboundItem.vue'
import UsualEntityService from '@/services/UsualEntityService'
import { _initialInboundOrder } from '@/utils/initialEntity'
import UserService from '@/services/UserService'

const route = useRoute()
const router = useRouter()
const id = route.params.id as string | undefined

const order = ref<InboundOrder>({ ..._initialInboundOrder })
const suppliers = ref<Supplier[]>([])
const formRef = ref<FormInstance>()

const loadOptions = async () => {
  try {
    const s = await SupplierService.GetSuppliers(1, 1000)
    suppliers.value = s.dataList || []
  } catch (e) {
    console.error(e)
  }
}

const loadOrder = async () => {
  if (!id) {
    //新建时调用自动编号
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
  } else
    try {
      const res = await UsualEntityService.Get<InboundOrder>('InboundOrder', id)
      if (res.stateCode && res.data) order.value = res.data as InboundOrder
    } catch (e) {
      console.error(e)
    }
}

const handleSave = async () => {
  try {
    await formRef.value?.validate?.()
    await InboundOrderService.EditInboundOrder(order.value)
    ElMessage.success('保存成功')
    router.back()
  } catch (e) {
    console.error(e)
  }
}

onMounted(async () => {
  await loadOptions()
  await loadOrder()
})

watch(
  () => route.params.id,
  async () => {
    await loadOrder()
  },
)
</script>

<template>
  <div class="apm-editor-container">
    <div class="apm-container">
      <div class="title">入库单 {{ id ? order.orderNo : '新建' }}</div>
      <el-form :model="order" ref="formRef" label-width="100px">
        <el-row :gutter="16">
          <el-col :span="6">
            <el-form-item label="订单号">
              <el-input v-model="order.orderNo" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="供应商">
              <el-select v-model="order.supplierId" placeholder="选择供应商">
                <el-option v-for="s in suppliers" :key="s.id" :label="s.name" :value="s.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="总金额">
              <el-input-number v-model="order.totalAmount" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="经办人">
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
        <el-button type="primary" size="normal" @click="handleSave">保存</el-button>
        <el-button type="normal" size="normal" @click="router.back">返回</el-button>
      </div>
    </div>
    <div class="apm-container">
      <InboundItem :orderId="order.id" />
    </div>
  </div>
</template>
