<script setup lang="ts">
import { ref, onMounted } from 'vue'
import InboundOrderService from '@/services/InboundOrderService'
import type { InboundOrder } from '@/interfaces/DTOEntities'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useRouter } from 'vue-router'
import { ConvertDateTime } from '@/utils/converter'
import UsualEntityService from '@/services/UsualEntityService'

const orders = ref<InboundOrder[] | null>([])
const pageIndex = ref(1)
const pageSize = ref(25)
const total = ref(0)
const selected = ref<InboundOrder[]>([])
const sortField = ref<string | null>(null)
const sortDesc = ref<boolean>(false)
const router = useRouter()

const load = async () => {
  try {
    const res = await InboundOrderService.GetInboundOrders(
      pageIndex.value,
      pageSize.value,
      sortField.value || undefined,
      sortDesc.value,
    )
    if (res.dataList) {
      orders.value = res.dataList || []
      total.value = res.total || 0
    }
  } catch (e) {
    console.error(e)
  }
}

const handleAdd = () => {
  router.push({ path: '/InboundOrder/edit' })
}

const handleEdit = (row: InboundOrder) => {
  router.push({ path: `/InboundOrder/edit/${row.id}` })
}

const handleDeleteSingle = async (row: InboundOrder) => {
  try {
    await ElMessageBox.confirm(`确定要删除入库单 ${row.orderNo} 吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const ids = [row.id!]
    await UsualEntityService.Delete('InboundOrder', ids)
    ElMessage.success('删除成功')
    load()
  } catch (err: any) {
    if (err !== 'cancel') ElMessage.error('删除失败')
  }
}

const handleBatchDelete = async () => {
  if (selected.value.length === 0) {
    ElMessage.warning('请先选择要删除的入库单')
    return
  }
  try {
    await ElMessageBox.confirm(`确定要删除选中的 ${selected.value.length} 个入库单吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const ids = selected.value.map((s) => s.id!)
    await UsualEntityService.Delete('InboundOrder', ids)
    ElMessage.success('删除成功')
    selected.value = []
    load()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error('删除失败')
  }
}

const handleSelectionChange = (sel: InboundOrder[]) => {
  selected.value = sel
}

const handleCurrentPageChange = (p: number) => {
  pageIndex.value = p
  load()
}

const handlePageSizeChange = (s: number) => {
  pageSize.value = s
  pageIndex.value = 1
  load()
}

onMounted(() => {
  load()
})
</script>

<template>
  <div class="apm-container">
    <div class="toolbar">
      <div class="toolbar-left">
        <h2>入库单管理</h2>
      </div>
      <div class="toolbar-right">
        <el-button type="primary" @click="handleAdd">新增</el-button>
        <el-button type="danger" @click="handleBatchDelete">删除</el-button>
      </div>
    </div>
    <el-table
      class="apm-table"
      stripe="true"
      :data="orders"
      @selection-change="handleSelectionChange"
      style="width: 100%"
    >
      <el-table-column type="selection" width="55" />
      <el-table-column prop="orderNo" label="订单号" />
      <el-table-column label="供应商">
        <template #default="{ row }">{{ row.supplier?.name }}</template>
      </el-table-column>
      <el-table-column prop="totalAmount" label="总金额" />
      <el-table-column label="经办人">
        <template #default="{ row }">{{ row.operatorUser?.realname }}</template>
      </el-table-column>
      <el-table-column label="创建时间">
        <template #default="{ row }">{{ ConvertDateTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="180">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="handleEdit(row)">编辑</el-button>
          <el-button link size="small" type="danger" @click="handleDeleteSingle(row)"
            >删除</el-button
          >
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination">
      <el-pagination
        :current-page="pageIndex"
        :page-size="pageSize"
        :page-sizes="[25, 50, 100]"
        :total="total"
        layout="total, sizes, prev, pager, next, jumper"
        @current-change="handleCurrentPageChange"
        @size-change="handlePageSizeChange"
      />
    </div>
  </div>
</template>
