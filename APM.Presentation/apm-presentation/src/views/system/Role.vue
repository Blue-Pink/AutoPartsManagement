<script setup lang="ts">
import { ref, onMounted } from 'vue'
import UsualEntityService from '@/services/UsualEntityService'
import type { Role } from '@/interfaces/Entities'
import { ElMessage, ElMessageBox } from 'element-plus'
import RoleEdit from '@/components/system/RoleEdit.vue'
import { ConvertDateTime } from '@/utils/converter'
import { _initialRole } from '@/utils/initialEntity'
import { ConstDictionary } from '@/utils/const-dictionary'

const roles = ref<Role[]>([])
const pageIndex = ref(1)
const pageSize = ref(25)
const total = ref(0)
const selected = ref<Role[]>([])
const editVisible = ref(false)
const editingRole = ref<Role>({ ..._initialRole })

const load = async () => {
  try {
    const res = await UsualEntityService.GetDataSet<Role>('Role', pageIndex.value, pageSize.value)
    if (res.dataList) {
      roles.value = res.dataList || []
      total.value = res.total || 0
    }
  } catch (e) {
    console.error(e)
  }
}

const handleAdd = () => {
  editingRole.value = { ..._initialRole }
  editVisible.value = true
}
const handleEdit = (row: Role) => {
  editingRole.value = { ...row }
  editVisible.value = true
}

const handleDeleteSingle = async (row: Role) => {
  try {
    await ElMessageBox.confirm(`确定要删除用户 ${row.roleName} 吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const ids = [(row as any).id]
    await UsualEntityService.Delete('Role', ids)
    ElMessage.success('删除成功')
    load()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error('删除失败')
  }
}

const handleBatchDelete = async () => {
  if (selected.value.length === 0) {
    ElMessage.warning('请先选择要删除的用户')
    return
  }
  try {
    await ElMessageBox.confirm(`确定要删除选中的 ${selected.value.length} 个用户吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const ids = selected.value.map((u) => (u as any).id)
    await UsualEntityService.Delete('Role', ids)
    ElMessage.success('删除成功')
    selected.value = []
    load()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error('删除失败')
  }
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
const handleSelectionChange = (sel: Role[]) => {
  selected.value = sel
}
const handleSaved = () => {
  editVisible.value = false
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
        <h2>角色管理</h2>
      </div>
      <div class="toolbar-right">
        <el-button type="primary" @click="handleAdd">新增</el-button>
        <el-button type="danger" @click="handleBatchDelete">删除</el-button>
      </div>
    </div>

    <el-table :data="roles" stripe @selection-change="handleSelectionChange" class="apm-table">
      <el-table-column type="selection" width="55" />
      <el-table-column prop="roleName" label="角色名" min-width="280" />
      <el-table-column prop="description" label="真实姓名" min-width="280" />
      <el-table-column
        prop="createdAt"
        label="创建时间"
        width="auto"
        min-width="180"
        sortable="custom"
      >
        <template #default="{ row }">
          {{ ConvertDateTime(row.createdAt, 'YYYY-MM-DD HH:mm:ss') }}
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
          {{ ConvertDateTime(row.modifiedAt, 'YYYY-MM-DD HH:mm:ss') }}
        </template>
      </el-table-column>
      <el-table-column label="操作" fixed="right" width="150">
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
        :total="total"
        :page-sizes="ConstDictionary.TABLE_PAGE_SIZES"
        layout="total, sizes, prev, pager, next, jumper"
        @current-change="handleCurrentPageChange"
        @size-change="handlePageSizeChange"
      />
    </div>

    <RoleEdit v-model="editVisible" :role="editingRole" @saved="handleSaved" />
  </div>
</template>
