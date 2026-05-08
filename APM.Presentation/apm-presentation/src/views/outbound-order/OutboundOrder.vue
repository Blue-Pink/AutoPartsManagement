<script setup lang="ts">
import { ref, onMounted } from 'vue'
import type { OutboundOrder } from '@/interfaces/Entities'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useRouter } from 'vue-router'
import { ConvertDateTime } from '@/utils/converter'
import UsualEntityService from '@/services/UsualEntityService'
import { ConstDictionary } from '@/utils/const-dictionary'

const orders = ref<OutboundOrder[] | null>([])
const pageIndex = ref(1)
const pageSize = ref(25)
const total = ref(0)
const selected = ref<OutboundOrder[]>([])
const orderBy = ref<string>('')
const descending = ref<boolean>(false)
const router = useRouter()

const load = async () => {
  try {
    const res = await UsualEntityService.GetDataSet<OutboundOrder>(
      'OutboundOrder',
      pageIndex.value,
      pageSize.value,
      orderBy.value,
      descending.value,
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
  router.push({ path: '/outbound-order/edit' })
}

const handleEdit = (row: OutboundOrder) => {
  router.push({ path: `/outbound-order/edit/${row.id}` })
}

const handleDeleteSingle = async (row: OutboundOrder) => {
  try {
    await ElMessageBox.confirm(`确定要删除出库单 ${row.orderNo} 吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const ids = [row.id!]
    await UsualEntityService.Delete('OutboundOrder', ids)
    ElMessage.success('删除成功')
    load()
  } catch (err: any) {
    if (err !== 'cancel') ElMessage.error('删除失败')
  }
}

const handleBatchDelete = async () => {
  if (selected.value.length === 0) {
    ElMessage.warning('请先选择要删除的出库单')
    return
  }
  try {
    await ElMessageBox.confirm(`确定要删除选中的 ${selected.value.length} 个出库单吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const ids = selected.value.map((s) => s.id!)
    await UsualEntityService.Delete('OutboundOrder', ids)
    ElMessage.success('删除成功')
    selected.value = []
    load()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error('删除失败')
  }
}

const handleSelectionChange = (sel: OutboundOrder[]) => {
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

const handleSortChange = (options: {
  column: any
  prop: string
  order: 'ascending' | 'descending' | null
}) => {
  console.log(options)
  if (!options || !options.prop) return
  if (options.order === 'ascending') {
    orderBy.value = options.prop
    descending.value = false
  } else if (options.order === 'descending') {
    orderBy.value = options.prop
    descending.value = true
  } else {
    orderBy.value = ''
    descending.value = false
  }
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
        <h2>出库单管理</h2>
      </div>
      <div class="toolbar-right">
        <el-button type="primary" @click="handleAdd">新增</el-button>
        <el-button type="danger" @click="handleBatchDelete">删除</el-button>
      </div>
    </div>
    <el-table
      class="apm-table"
      stripe
      :data="orders"
      @selection-change="handleSelectionChange"
      @sort-change="handleSortChange"
    >
      <el-table-column type="selection" width="55" />
      <el-table-column prop="orderNo" label="订单号" />
      <el-table-column prop="customer" label="客户" sortable="custom">
        <template #default="{ row }">{{ row.customer?.name }}</template>
      </el-table-column>
      <el-table-column prop="totalAmount" label="总金额" />
      <el-table-column prop="outboundDate" label="出库时间" sortable="custom">
        <template #default="{ row }">{{ ConvertDateTime(row.outboundDate) }}</template>
      </el-table-column>
      <el-table-column prop="createdAt" label="创建时间" sortable="custom">
        <template #default="{ row }">{{ ConvertDateTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column prop="modifiedAt" label="修改时间" sortable="custom">
        <template #default="{ row }">{{ ConvertDateTime(row.modifiedAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="180">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="handleEdit(row)" v-text="'编辑'" />
          <el-button
            link
            size="small"
            type="danger"
            @click="handleDeleteSingle(row)"
            v-text="'删除'"
          />
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination">
      <el-pagination
        :current-page="pageIndex"
        :page-size="pageSize"
        :page-sizes="ConstDictionary.TABLE_PAGE_SIZES"
        :total="total"
        layout="total, sizes, prev, pager, next, jumper"
        @current-change="handleCurrentPageChange"
        @size-change="handlePageSizeChange"
      />
    </div>
  </div>
</template>
