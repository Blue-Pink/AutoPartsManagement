<script setup lang="ts">
import { ref, onMounted } from 'vue'
import UserService from '@/services/UserService'
import UsualEntityService from '@/services/UsualEntityService'
import type { InboundOrder, Role, User } from '@/interfaces/DTOEntities'
import { ElMessage, ElMessageBox } from 'element-plus'
import UserEdit from '@/components/user/UserEdit.vue'
import { ConvertDateTime } from '@/utils/converter'
import { createDefaultEntity, _initialUser } from '@/utils/initialEntity'

const users = ref<User[]>([])
const pageIndex = ref(1)
const pageSize = ref(25)
const total = ref(0)
const selected = ref<InboundOrder[]>([])
const editVisible = ref(false)
const editingUser = ref<User>({ ..._initialUser })

const load = async () => {
  try {
    const res = await UserService.GetUsers(pageIndex.value, pageSize.value)
    if (res.dataList) {
      users.value = res.dataList || []
      total.value = res.total || 0
    }
  } catch (e) {
    console.error(e)
  }
}

const handleAdd = () => {
  editingUser.value = { ..._initialUser }
  editVisible.value = true
}
const handleEdit = (row: User) => {
  editingUser.value = { ...row }
  editVisible.value = true
}

const handleDeleteSingle = async (row: User) => {
  try {
    await ElMessageBox.confirm(`确定要删除用户 ${row.username} 吗？`, '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const ids = [(row as any).id]
    await UsualEntityService.Delete('User', ids)
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
    await UsualEntityService.Delete('User', ids)
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
const handleSelectionChange = (sel: InboundOrder[]) => {
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
        <h2>用户管理</h2>
      </div>
      <div class="toolbar-right">
        <el-button type="primary" @click="handleAdd">新增</el-button>
        <el-button type="danger" @click="handleBatchDelete">删除</el-button>
      </div>
    </div>

    <el-table
      :data="users"
      stripe="true"
      @selection-change="handleSelectionChange"
      class="apm-table"
    >
      <el-table-column type="selection" width="55" />
      <el-table-column prop="username" label="用户名" min-width="280" />
      <el-table-column prop="realname" label="真实姓名" min-width="280" />
      <!--角色-->
      <el-table-column prop="role" label="角色" min-width="220" width="auto">
        <template #default="{ row }">
          <div class="flex gap-2">
            <el-tag v-for="r in row.roles" :key="r.id">{{ r.description }}</el-tag>
          </div>
        </template>
      </el-table-column>
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
      <el-table-column label="操作" fixed="right" width="150">
        <template #default="{ row }">
          <el-button link size="mini" type="primary" @click="handleEdit(row)">编辑</el-button>
          <el-button link size="mini" type="danger" @click="handleDeleteSingle(row)"
            >删除</el-button
          >
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination">
      <el-pagination
        :current-page="pageIndex"
        :page-size="pageSize"
        :total="total"
        :page-sizes="[25, 50, 100]"
        layout="total, sizes, prev, pager, next, jumper"
        @current-change="handleCurrentPageChange"
        @size-change="handlePageSizeChange"
      />
    </div>

    <UserEdit v-model="editVisible" :user="editingUser" @saved="handleSaved" />
  </div>
</template>
