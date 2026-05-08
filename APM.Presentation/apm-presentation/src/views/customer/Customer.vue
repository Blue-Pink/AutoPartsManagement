<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import type { Customer } from '@/interfaces/Entities'
import { ElMessage, ElMessageBox } from 'element-plus'
import { ConvertDateTime } from '@/utils/converter'
import CustomerEdit from '@/components/customer/CustomerEdit.vue'
import UsualEntityService from '@/services/UsualEntityService'
import { _initialCustomer } from '@/utils/initialEntity'
import { ConstDictionary } from '@/utils/const-dictionary'

const customers = ref<Customer[] | null>([])
const pageIndex = ref(1)
const pageSize = ref(25)
const total = ref(0)
const selectedCustomers = ref<Customer[]>([])
const editVisible = ref(false)
const editingCustomer = ref<Customer>({ ..._initialCustomer })
const orderBy = ref<string>('')
const descending = ref<boolean>(false)

const loadCustomers = async () => {
  try {
    const c = await UsualEntityService.GetDataSet<Customer>(
      'Customer',
      pageIndex.value,
      pageSize.value,
      orderBy.value,
      descending.value,
    )
    if (c.dataList) {
      customers.value = c.dataList || []
      total.value = c.total || 0
    }
  } catch (error) {
    console.error('加载客户列表失败', error)
  }
}

const handleCurrentPageChange = (page: number) => {
  pageIndex.value = page
}

const handlePageSizeChange = (size: number) => {
  pageSize.value = size
  pageIndex.value = 1
}

const handleAddCustomer = () => {
  editingCustomer.value = { ..._initialCustomer }
  editVisible.value = true
}

const handleEditCustomer = (row: Customer) => {
  editingCustomer.value = { ...row }
  editVisible.value = true
}

const handleDeleteSingle = async (row: Customer) => {
  try {
    await ElMessageBox.confirm(`确定要删除客户 [${row.name}] 吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const ids = [row.id!]
    await UsualEntityService.Delete('Customer', ids)
    ElMessage.success('删除成功')
    loadCustomers()
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败')
      console.error('删除客户失败', error)
    }
  }
}

const handleBatchDelete = async () => {
  if (selectedCustomers.value.length === 0) {
    ElMessage.warning('请先选择要删除的客户')
    return
  }

  try {
    await ElMessageBox.confirm(
      `确定要删除选中的 ${selectedCustomers.value.length} 个客户吗？`,
      '警告',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )

    const ids = selectedCustomers.value.map((customer) => customer.id!)
    await UsualEntityService.Delete('Customer', ids)
    ElMessage.success('删除成功')
    selectedCustomers.value = []
    loadCustomers()
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败')
      console.error('删除客户失败', error)
    }
  }
}

const handleSelectionChange = (selection: Customer[]) => {
  selectedCustomers.value = selection
}

const handleSortChange = (options: {
  column: any
  prop: string
  order: 'ascending' | 'descending' | null
}) => {
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
  loadCustomers()
}

onMounted(loadCustomers)

watch([pageIndex, pageSize], loadCustomers)
</script>

<template>
  <div class="apm-container">
    <div class="toolbar">
      <div class="toolbar-left">
        <h2>客户管理</h2>
      </div>
      <div class="toolbar-right">
        <el-button type="primary" @click="handleAddCustomer">新增</el-button>
        <el-button type="danger" @click="handleBatchDelete">删除</el-button>
      </div>
    </div>

    <el-table
      class="apm-table"
      :data="customers"
      stripe
      @selection-change="handleSelectionChange"
      @sort-change="handleSortChange"
    >
      <el-table-column type="selection" width="55" />
      <el-table-column prop="name" label="名称" width="180" sortable="custom" />
      <el-table-column prop="contactPerson" label="联系人" width="140" />
      <el-table-column prop="phone" label="电话" width="160" />
      <el-table-column prop="address" label="地址" width="240" />
      <el-table-column prop="remark" label="备注" width="240" />
      <el-table-column
        prop="createdAt"
        label="创建时间"
        width="auto"
        min-width="180"
        sortable="custom"
      >
        <template #default="{ row }">{{
          ConvertDateTime(row.createdAt, 'YYYY-MM-DD HH:mm:ss')
        }}</template>
      </el-table-column>
      <el-table-column
        prop="modifiedAt"
        label="修改时间"
        width="auto"
        min-width="180"
        sortable="custom"
      >
        <template #default="{ row }">{{
          ConvertDateTime(row.modifiedAt, 'YYYY-MM-DD HH:mm:ss')
        }}</template>
      </el-table-column>
      <el-table-column label="操作" width="150" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="handleEditCustomer(row)"
            >编辑</el-button
          >
          <el-button link type="danger" size="small" @click="handleDeleteSingle(row)"
            >删除</el-button
          >
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination">
      <el-pagination
        v-model:current-page="pageIndex"
        v-model:page-size="pageSize"
        :page-sizes="ConstDictionary.TABLE_PAGE_SIZES"
        :total="total"
        layout="total, sizes, prev, pager, next, jumper"
        @current-page-change="handleCurrentPageChange"
        @page-size-change="handlePageSizeChange"
      />
    </div>

    <CustomerEdit v-model="editVisible" :customer="editingCustomer" @saved="loadCustomers" />
  </div>
</template>

<style scoped></style>
