<script setup lang="ts">
import { ref, onMounted } from 'vue'
import UsualEntityService from '@/services/UsualEntityService'
import type {
  EntityAndRolePermission,
  EntityRecord,
  Role,
  RolePermission,
} from '@/interfaces/Entities'
import { ElMessage } from 'element-plus'
import { ConvertDateTime } from '@/utils/converter'
import { _initialRole, _initialRolePermission } from '@/utils/initialEntity'
import { ConstDictionary } from '@/utils/const-dictionary'

const rolePermissions = ref<RolePermission[]>([])
const entityRecords = ref<EntityRecord[]>([])
const entityAndRolePermissions = ref<EntityAndRolePermission[]>([])
const pageIndex = ref(1)
const pageSize = ref(25)
const total = ref(0)
const roles = ref<Role[]>([])
const currentRoleId = ref<string>('')
const orderBy = ref<string>('isActive')
const descending = ref<boolean>(false)

const load = async () => {
  try {
    if (!currentRoleId || !currentRoleId.value) return
    const res = await UsualEntityService.GetDataSet<RolePermission>(
      'RolePermission',
      pageIndex.value,
      pageSize.value,
      '',
      descending.value,
      currentRoleId.value,
    )

    if (res.dataList) {
      rolePermissions.value = res.dataList || []
      total.value = res.total || 0
    }

    const entitiesRes = await UsualEntityService.GetDataSet<EntityRecord>(
      'EntityRecord',
      0,
      0,
      orderBy.value,
      true,
    )
    entityRecords.value = entitiesRes.dataList || []

    if (entityRecords.value.length > 0) {
      entityAndRolePermissions.value = [...entityRecords.value] as EntityAndRolePermission[]
      entityAndRolePermissions.value = entityAndRolePermissions.value.map((e) => {
        const rp = rolePermissions.value.find(
          (rp) => rp.entityId === e.id && rp.roleId === currentRoleId.value,
        )
        if (rp) {
          e = { ...e, ...rp }
        } else {
          e.entityId = e.id
          e.id = ConstDictionary.EMPTY_GUID
          e.canRead = false
          e.canCreate = false
          e.canUpdate = false
          e.canDelete = false
          e.roleId = currentRoleId.value
        }
        return e
      })
    }
  } catch (e) {
    console.error(e)
  }
}

const loadRoles = async () => {
  try {
    const res = await UsualEntityService.GetDataSet<Role>('Role', 0)
    if (res.dataList) {
      roles.value = res.dataList || []
    }
  } catch (e) {
    console.error(e)
  }
}

const handlePermissionChange = async (
  permission: RolePermission,
  field: 'canRead' | 'canCreate' | 'canUpdate' | 'canDelete',
) => {
  try {
    // 构建更新数据
    const updateData = {
      ...permission,
      [field]: !permission[field],
    }

    // 调用更新接口
    await UsualEntityService.Edit('RolePermission', updateData)
    await load()

    // 更新本地数据
    permission[field] = !permission[field]
    ElMessage.success('权限更新成功')
  } catch (e: any) {
    console.error(e)
  }
}

const handleAllowAll = async (row: RolePermission) => {
  try {
    const updateData = {
      ...row,
      canRead: true,
      canCreate: true,
      canUpdate: true,
      canDelete: true,
    }
    await UsualEntityService.Edit('RolePermission', updateData)
    await load()
    ElMessage.success('已全部允许')
  } catch (e: any) {
    console.error(e)
  }
}

const handleBlockAll = async (row: RolePermission) => {
  try {
    const updateData = {
      ...row,
      canRead: false,
      canCreate: false,
      canUpdate: false,
      canDelete: false,
    }
    await UsualEntityService.Edit('RolePermission', updateData)
    await load()
    ElMessage.success('已全部拦截')
  } catch (e: any) {
    console.error(e)
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

onMounted(loadRoles)
</script>

<template>
  <div class="apm-container">
    <div class="toolbar">
      <div class="toolbar-left">
        <h2>角色权限管理</h2>
      </div>
      <div class="toolbar-right">
        <el-select v-model="currentRoleId" placeholder="选择一名角色" @change="load">
          <el-option v-for="s in roles" :key="s.id" :label="s.roleName" :value="s.id" />
        </el-select>
      </div>
    </div>

    <el-table
      :data="entityAndRolePermissions"
      stripe
      class="apm-table"
      @sort-change="handleSortChange"
    >
      <el-table-column type="selection" width="55" />
      <el-table-column prop="entityName" label="实体名称" min-width="120" sortable="custom" />
      <el-table-column prop="fullName" label="完整名称" min-width="180" sortable="custom" />
      <el-table-column prop="description" label="备注" min-width="180" sortable="custom" />
      <el-table-column label="实体启用" width="100" align="center">
        <template #default="{ row }">
          <el-checkbox :model-value="row.isActive" disabled />
        </template>
      </el-table-column>
      <el-table-column label="查看" width="100" align="center">
        <template #default="{ row }">
          <el-checkbox
            :model-value="row.canRead"
            @change="handlePermissionChange(row, 'canRead')"
          />
        </template>
      </el-table-column>

      <el-table-column label="新增" width="100" align="center">
        <template #default="{ row }">
          <el-checkbox
            :model-value="row.canCreate"
            @change="handlePermissionChange(row, 'canCreate')"
          />
        </template>
      </el-table-column>

      <el-table-column label="修改" width="100" align="center">
        <template #default="{ row }">
          <el-checkbox
            :model-value="row.canUpdate"
            @change="handlePermissionChange(row, 'canUpdate')"
          />
        </template>
      </el-table-column>

      <el-table-column label="删除" width="100" align="center">
        <template #default="{ row }">
          <el-checkbox
            :model-value="row.canDelete"
            @change="handlePermissionChange(row, 'canDelete')"
          />
        </template>
      </el-table-column>

      <el-table-column prop="createdAt" label="创建时间" width="180" sortable="custom">
        <template #default="{ row }">
          {{ ConvertDateTime(row.createdAt, 'YYYY-MM-DD HH:mm:ss') }}
        </template>
      </el-table-column>

      <el-table-column prop="modifiedAt" label="修改时间" width="180" sortable="custom">
        <template #default="{ row }">
          {{ ConvertDateTime(row.modifiedAt, 'YYYY-MM-DD HH:mm:ss') }}
        </template>
      </el-table-column>

      <el-table-column label="操作" fixed="right" width="200">
        <template #default="{ row }">
          <el-button
            link
            size="small"
            type="success"
            @click="handleAllowAll(row)"
            v-text="'全部允许'"
          />
          <el-button
            link
            size="small"
            type="danger"
            @click="handleBlockAll(row)"
            v-text="'全部拦截'"
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
  </div>
</template>
<style scoped>
.toolbar-right {
  flex-shrink: 0;
  width: 250px;
}
</style>
