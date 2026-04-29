<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import PartService from '@/services/PartService'
import type { Part } from '@/interfaces/DTOEntities'
import { ElMessage, ElMessageBox } from 'element-plus'
import { ConvertDateTime } from '@/utils/converter'
import PartEdit from '@/components/part/PartEdit.vue'
import UsualEntityService from '@/services/UsualEntityService'
import { _initialPart } from '@/utils/initialEntity'

const parts = ref<Part[] | null>([])
const pageIndex = ref(1)
const pageSize = ref(25)
const total = ref(0)
const selectedParts = ref<Part[]>([])
const editVisible = ref(false)
const editingPart = ref<Part>({ ..._initialPart })
const sortField = ref<string | null>(null)
const sortDesc = ref<boolean>(false)

const loadParts = async () => {
  try {
    const res = await PartService.GetParts(
      pageIndex.value,
      pageSize.value,
      sortField.value || undefined,
      sortDesc.value,
    )
    if (res.dataList) {
      parts.value = res.dataList || []
      total.value = res.total || 0
    }
  } catch (error) {
    console.error('加载配件列表失败', error)
  }
}

const handleCurrentPageChange = (page: number) => {
  pageIndex.value = page
  loadParts()
}

const handlePageSizeChange = (size: number) => {
  pageSize.value = size
  pageIndex.value = 1
  loadParts()
}

const handleAddPart = () => {
  editingPart.value = { ..._initialPart }
  editVisible.value = true
}

const handleEditPart = (row: Part) => {
  editingPart.value = { ...row }
  editVisible.value = true
}

const handleDeleteSingle = async (row: Part) => {
  try {
    await ElMessageBox.confirm(`确定要删除配件 ${row.model} 吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const ids = [row.id!]
    await UsualEntityService.Delete('Part', ids)
    ElMessage.success('删除成功')
    loadParts()
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败')
      console.error('删除配件失败', error)
    }
  }
}

const handleBatchDelete = async () => {
  if (selectedParts.value.length === 0) {
    ElMessage.warning('请先选择要删除的配件')
    return
  }

  try {
    await ElMessageBox.confirm(
      `确定要删除选中的 ${selectedParts.value.length} 个配件吗？`,
      '警告',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )

    const ids = selectedParts.value.map((part) => part.id!)
    await UsualEntityService.Delete('Part', ids)
    ElMessage.success('删除成功')
    selectedParts.value = []
    loadParts()
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败')
      console.error('删除配件失败', error)
    }
  }
}

const handleSelectionChange = (selection: Part[]) => {
  selectedParts.value = selection
}

const handleSortChange = (options: {
  column: any
  prop: string
  order: 'ascending' | 'descending' | null
}) => {
  if (!options || !options.prop) return
  if (options.order === 'ascending') {
    sortField.value = options.prop
    sortDesc.value = false
  } else if (options.order === 'descending') {
    sortField.value = options.prop
    sortDesc.value = true
  } else {
    sortField.value = null
    sortDesc.value = false
  }
  pageIndex.value = 1
  loadParts()
}

onMounted(loadParts)

watch([pageIndex, pageSize], loadParts)
</script>

<template>
  <div class="apm-container">
    <div class="toolbar">
      <div class="toolbar-left">
        <h2>配件管理</h2>
      </div>
      <div class="toolbar-right">
        <el-button type="primary" @click="handleAddPart">新增</el-button>
        <el-button type="danger" @click="handleBatchDelete">删除</el-button>
      </div>
    </div>
    <el-table
      class="apm-table"
      :data="parts"
      stripe="true"
      @selection-change="handleSelectionChange"
      @sort-change="handleSortChange"
    >
      <el-table-column type="selection" width="55" />
      <el-table-column prop="partName" label="名称" width="180" sortable="custom" />
      <el-table-column prop="oeCode" label="OE代码" width="180" sortable="custom" />
      <el-table-column prop="model" label="型号" width="180" />
      <el-table-column prop="brand" label="品牌" width="180" />
      <el-table-column prop="categoryName" label="分类" width="180" />
      <el-table-column prop="unitName" label="单位" width="100" />
      <el-table-column prop="costPrice" label="成本价" width="120" sortable="custom" />
      <el-table-column prop="sellingPrice" label="售价" width="120" sortable="custom" />
      <el-table-column prop="minStock" label="最小库存" width="120" sortable="custom" />
      <el-table-column prop="maxStock" label="最大库存" width="120" sortable="custom" />
      <el-table-column
        prop="createdAt"
        label="创建时间"
        width="auto"
        min-width="180"
        sortable="custom"
      >
        <template #default="{ row }">
          {{ ConvertDateTime(row.createdAt, 'yyyy-mm-dd hh:mm:ss') }}
        </template>
      </el-table-column>
      <el-table-column
        prop="modifiedAt"
        label="修改时间"
        width="auto"
        min-width="180"
        sortable="custom"
      >
        <template #default="{ row }">
          {{ ConvertDateTime(row.modifiedAt, 'yyyy-mm-dd hh:mm:ss') }}
        </template>
      </el-table-column>
      <el-table-column label="操作" width="150" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="handleEditPart(row)">编辑</el-button>
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

    <PartEdit v-model="editVisible" :part="editingPart" @saved="loadParts" />
  </div>
</template>

<style scoped></style>
