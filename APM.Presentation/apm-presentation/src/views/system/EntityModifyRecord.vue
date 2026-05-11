<script setup lang="ts">
import { ref, onMounted } from 'vue'
import UsualEntityService from '@/services/UsualEntityService'
import type { EntityModifyRecord, EntityRecord } from '@/interfaces/Entities'
import { ConvertDateTime } from '@/utils/converter'
import { _initialRole, _initialRolePermission } from '@/utils/initialEntity'
import { ConstDictionary } from '@/utils/const-dictionary'

const entityModifyRecords = ref<EntityModifyRecord[]>([])
const entities = ref<EntityRecord[]>([])
const pageIndex = ref(1)
const pageSize = ref(25)
const total = ref(0)
const entityId = ref<string>('')
const orderBy = ref<string>('')
const descending = ref<boolean>(true)

const load = async () => {
  try {
    const res = await UsualEntityService.GetDataSet<EntityModifyRecord>(
      'EntityModifyRecord',
      pageIndex.value,
      pageSize.value,
      orderBy.value,
      descending.value,
      entityId.value,
    )
    entityModifyRecords.value = res.dataList || []
  } catch (e) {
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
    descending.value = true
  }
  pageIndex.value = 1
  load()
}

const loadEntities = async () => {
  try {
    const res = await UsualEntityService.GetDataSet<EntityRecord>('EntityRecord', 0)
    entities.value = res.dataList || []
  } catch (e) {
    console.error(e)
  }
}

onMounted(async () => {
  await loadEntities()
  await load()
})
</script>

<template>
  <div class="apm-container">
    <div class="toolbar">
      <div class="toolbar-left">
        <h2>数据变更记录</h2>
      </div>
      <div class="toolbar-right">
        <div class="toolbar-right">
          <el-select v-model="entityId" placeholder="选择一个实体" @change="load" clearable>
            <el-option v-for="e in entities" :key="e.id" :label="e.entityName" :value="e.id" />
          </el-select>
        </div>
      </div>
    </div>

    <el-table :data="entityModifyRecords" stripe class="apm-table" @sort-change="handleSortChange">
      <el-table-column prop="entity" label="实体" width="120" sortable="custom">
        <template #default="{ row }">
          {{ row.entity.entityName }}
        </template>
      </el-table-column>
      <el-table-column prop="entity" label="实体名称" width="120" sortable="custom">
        <template #default="{ row }">
          {{ row.entity.description }}
        </template>
      </el-table-column>
      <el-table-column prop="operation" label="操作" width="120" sortable="custom" />
      <el-table-column prop="fieldName" label="字段" width="120" sortable="custom" />
      <el-table-column prop="oldValue" label="旧值" min-width="180" sortable="custom" />
      <el-table-column prop="newValue" label="新值" min-width="180" sortable="custom" />

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
