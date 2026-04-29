<script setup lang="ts">
import { ref, onMounted } from 'vue'
import SupplierService from '@/services/SupplierService'
import type { Supplier } from '@/interfaces/DTOEntities'
import { ElMessage, ElMessageBox } from 'element-plus'
import SupplierEdit from '@/components/inbound-order/SupplierEdit.vue'
import { ConvertDateTime } from '@/utils/converter'
import UsualEntityService from '@/services/UsualEntityService'
import { _initialSupplier } from '@/utils/initialEntity'

const suppliers = ref<Supplier[] | null>([])
const pageIndex = ref(1)
const pageSize = ref(25)
const total = ref(0)
const selectedSuppliers = ref<Supplier[]>([])
const editVisible = ref(false)
const editingSupplier = ref<Supplier>({ ..._initialSupplier })
const sortField = ref<string | null>(null)
const sortDesc = ref<boolean>(false)

const loadSuppliers = async () => {
  try {
    const res = await SupplierService.GetSuppliers(
      pageIndex.value,
      pageSize.value,
      sortField.value || undefined,
      sortDesc.value,
    )
    if (res.dataList) {
      suppliers.value = res.dataList || []
      total.value = res.total || 0
    }
  } catch (error) {
    console.error('加载供应商列表失败', error)
  }
}

const handleCurrentPageChange = (page: number) => {
  pageIndex.value = page
  loadSuppliers()
}

const handlePageSizeChange = (size: number) => {
  pageSize.value = size
  pageIndex.value = 1
  loadSuppliers()
}

const handleAdd = () => {
  editingSupplier.value = { ..._initialSupplier }
  editVisible.value = true
}

const handleEditSupplier = (row: Supplier) => {
  editingSupplier.value = { ...row }
  editVisible.value = true
}

const handleDeleteSingle = async (row: Supplier) => {
  try {
    await ElMessageBox.confirm(`确定要删除供应商 ${row.name} 吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const ids = [row.id!]
    await UsualEntityService.Delete('Supplier', ids)
    ElMessage.success('删除成功')
    loadSuppliers()
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败')
      console.error('删除供应商失败', error)
    }
  }
}

const handleBatchDelete = async () => {
  if (selectedSuppliers.value.length === 0) {
    ElMessage.warning('请先选择要删除的供应商')
    return
  }

  try {
    await ElMessageBox.confirm(
      `确定要删除选中的 ${selectedSuppliers.value.length} 个供应商吗？`,
      '警告',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )

    const ids = selectedSuppliers.value.map((s) => s.id!)
    await UsualEntityService.Delete('Supplier', ids)
    ElMessage.success('删除成功')
    selectedSuppliers.value = []
    loadSuppliers()
  } catch (error: any) {
    if (error !== 'cancel') {
      console.error('删除供应商失败', error)
    }
  }
}

const handleSelectionChange = (selection: Supplier[]) => {
  selectedSuppliers.value = selection
}

onMounted(() => {
  loadSuppliers()
})
</script>

<template>
  <div class="apm-container">
    <div class="toolbar">
      <div class="toolbar-left">
        <h2>供应商管理</h2>
      </div>
      <div class="toolbar-right">
        <el-button type="primary" @click="handleAdd">新增</el-button>
        <el-button type="danger" @click="handleBatchDelete">删除</el-button>
      </div>
    </div>

    <el-table
      class="apm-table"
      stripe="true"
      :data="suppliers"
      @selection-change="handleSelectionChange"
    >
      <el-table-column type="selection" width="55"></el-table-column>
      <el-table-column prop="name" label="名称" width="180"></el-table-column>
      <el-table-column prop="contact" label="联系人" width="120"></el-table-column>
      <el-table-column prop="phone" label="电话" width="160"></el-table-column>
      <el-table-column prop="address" label="地址" min-width="320" width="auto"></el-table-column>
      <el-table-column prop="createdAt" label="创建时间" min-width="180" width="auto">
        <template #default="{ row }">{{ ConvertDateTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column prop="modifiedAt" label="修改时间" min-width="180" width="auto">
        <template #default="{ row }">{{ ConvertDateTime(row.modifiedAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="150" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="handleEditSupplier(row)"
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
        :page-sizes="[25, 50, 100]"
        :total="total"
        layout="total, sizes, prev, pager, next, jumper"
        @current-page-change="handleCurrentPageChange"
        @page-size-change="handlePageSizeChange"
      />
    </div>

    <supplier-edit v-model="editVisible" :supplier="editingSupplier" @saved="loadSuppliers" />
  </div>
</template>
