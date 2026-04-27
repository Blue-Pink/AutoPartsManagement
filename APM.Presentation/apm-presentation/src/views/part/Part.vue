<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import PartService from '@/services/PartService'
import type { PartView } from '@/interfaces/DTOEntities'
import { ElMessage, ElMessageBox, ElLoading } from 'element-plus'
import { ConvertDateTime } from '@/utils/converter'
import PartEdit from '@/components/part/PartEdit.vue'

const parts = ref<PartView[] | null>([])
const pageIndex = ref(1)
const pageSize = ref(25)
const total = ref(0)
const selectedParts = ref<PartView[]>([])
const editVisible = ref(false)
const editingPart = ref<PartView | null>(null)

const loadParts = async () => {
  const loadingInst = ElLoading.service({ lock: true, text: '加载中...' })
  try {
    const res = await PartService.GetParts(pageIndex.value, pageSize.value)
    if (res.dataList) {
      parts.value = res.dataList || []
      total.value = res.total || 0
    }
  } catch (error) {
    ElMessage.error('加载配件列表失败')
    console.error('加载配件列表失败', error)
  } finally {
    loadingInst && loadingInst.close && loadingInst.close()
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
  editingPart.value = null
  editVisible.value = true
}

const handleEditPart = (row: PartView) => {
  editingPart.value = { ...row }
  editVisible.value = true
}

const handleDeleteSingle = async (row: PartView) => {
  try {
    await ElMessageBox.confirm(`确定要删除配件 ${row.model} 吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const ids = [row.id!]
    const loadingInst = ElLoading.service({ lock: true, text: '删除中...' })
    try {
      await PartService.DeleteParts(ids)
      ElMessage.success('删除成功')
      loadParts()
    } finally {
      loadingInst && loadingInst.close && loadingInst.close()
    }
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
    const loadingInst = ElLoading.service({ lock: true, text: '删除中...' })
    try {
      await PartService.DeleteParts(ids)
      ElMessage.success('删除成功')
      selectedParts.value = []
      loadParts()
    } finally {
      loadingInst && loadingInst.close && loadingInst.close()
    }
  } catch (error: any) {
    if (error !== 'cancel') {
      ElMessage.error('删除失败')
      console.error('删除配件失败', error)
    }
  }
}

const handleSelectionChange = (selection: PartView[]) => {
  selectedParts.value = selection
}

onMounted(loadParts)

watch([pageIndex, pageSize], loadParts)
</script>

<template>
  <div class="part-container">
    <div class="toolbar">
      <div class="toolbar-left">
        <h2>配件列表</h2>
      </div>
      <div class="toolbar-right">
        <el-button type="primary" @click="handleAddPart">新增</el-button>
        <el-button type="danger" @click="handleBatchDelete">删除</el-button>
      </div>
    </div>
    <el-table
      :data="parts"
      stripe="true"
      style="width: 100%"
      height="82vh"
      border="true"
      @selection-change="handleSelectionChange"
    >
      <el-table-column type="selection" width="55" />
      <el-table-column prop="partName" label="名称" width="180" />
      <el-table-column prop="oeCode" label="OE代码" width="180" />
      <el-table-column prop="model" label="型号" width="180" />
      <el-table-column prop="brand" label="品牌" width="180" />
      <el-table-column prop="categoryName" label="分类" width="180" />
      <el-table-column prop="unitName" label="单位" width="100" />
      <el-table-column prop="costPrice" label="成本价" width="120" />
      <el-table-column prop="sellingPrice" label="售价" width="120" />
      <el-table-column prop="minStock" label="最小库存" width="120" />
      <el-table-column prop="maxStock" label="最大库存" width="120" />
      <el-table-column label="创建时间" width="auto" min-width="180">
        <template #default="{ row }">
          {{ ConvertDateTime(row.createdAt, 'yyyy-mm-dd hh:mm:ss') }}
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
    <PartEdit v-model="editVisible" :part="editingPart" @saved="loadParts" />

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
  </div>
</template>

<style scoped>
.part-container {
  box-shadow: var(--basic-box-shadow);
  background-color: var(--basic-color-white);
  padding: 20px;
  border-radius: 4px;
}

.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.toolbar-left h2 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
}

.toolbar-right {
  display: flex;
  gap: 10px;
}

.pagination {
  margin-top: 20px;
  display: flex;
  justify-content: center;
}
</style>
