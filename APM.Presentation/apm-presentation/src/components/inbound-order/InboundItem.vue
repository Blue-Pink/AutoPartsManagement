<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import type { InboundItem } from '@/interfaces/DTOEntities'
import { ElMessage, ElMessageBox } from 'element-plus'
import InboundItemEdit from '@/components/inbound-order/InboundItemEdit.vue'
import UsualEntityService from '@/services/UsualEntityService'
import { ConstDictionary } from '@/utils/const-dictionary'
import { _initialInboundItem } from '@/utils/initialEntity'
import { ConvertDateTime } from '@/utils/converter'

const props = defineProps<{ orderId: string | null }>()
const pageIndex = ref(1)
const pageSize = ref(5)
const sortField = ref<string | null>(null)
const sortDesc = ref<boolean>(false)
const total = ref(0)
const items = ref<InboundItem[]>([])
const editVisible = ref(false)
const editing = ref<InboundItem>({ ..._initialInboundItem })
const selected = ref<InboundItem[]>([])
const emit = defineEmits(['saved'])

const load = async () => {
  console.log('加载入库单明细，orderId=', props.orderId)
  if (!props.orderId) return
  try {
    if (props.orderId && props.orderId !== ConstDictionary.EMPTY_GUID) {
      const res = await UsualEntityService.GetChildrenDataSet<InboundItem>(
        'InboundOrder',
        'InboundItem',
        props.orderId,
        pageIndex.value,
        pageSize.value,
        sortField.value || undefined,
        sortDesc.value,
      )
      items.value = res.dataList || []
      total.value = res.total || 0
    }
  } catch (e) {
    console.error(e)
  }
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
  load()
}

const handleAdd = () => {
  editing.value = { ..._initialInboundItem }
  editVisible.value = true
}

const handleSelectionChange = (sel: InboundItem[]) => {
  selected.value = sel
}

const handleEdit = (row: InboundItem) => {
  editing.value = { ...row }
  editVisible.value = true
}

const handleDelete = async (row: InboundItem) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除明细 [${row.part?.model}] - [${row.part?.partName}] 吗？`,
      '警告',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )
    const ids = [row.id!]
    await UsualEntityService.Delete('InboundItem', ids)
    ElMessage.success('删除成功')
    load()
    emit('saved')
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error('删除失败')
  }
}

const handleBatchDelete = async () => {
  if (selected.value.length === 0) {
    ElMessage.warning('请先选择要删除的入库单明细')
    return
  }
  try {
    await ElMessageBox.confirm(
      `确定要删除选中的 ${selected.value.length} 个入库单明细吗？`,
      '警告',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )
    const ids = selected.value.map((s) => s.id!)
    await UsualEntityService.Delete('InboundItem', ids)
    ElMessage.success('删除成功')
    selected.value = []
    load()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error('删除失败')
  }
}

const handleSaved = () => {
  editVisible.value = false
  load()
  emit('saved')
}

const handleCurrentPageChange = (page: number) => {
  pageIndex.value = page
  load()
}

const handlePageSizeChange = (size: number) => {
  pageSize.value = size
  pageIndex.value = 1
}

watch(
  () => props.orderId,
  async () => await load(),
)

watch([pageIndex, pageSize], load)
</script>

<template>
  <div class="apm-table-container">
    <div class="toolbar">
      <div class="toolbar-left">
        <h2>入库单明细管理</h2>
      </div>
      <div class="toolbar-right">
        <el-button type="primary" @click="handleAdd">添加明细</el-button>
        <el-button type="danger" @click="handleBatchDelete">删除</el-button>
      </div>
    </div>
    <el-table
      :data="items"
      class="apm-table"
      @selection-change="handleSelectionChange"
      @sort-change="handleSortChange"
    >
      <el-table-column type="selection" width="55" />
      <el-table-column label="配件">
        <template #default="{ row }">{{
          `[${row.part?.model}] - [${row.part?.partName}]`
        }}</template>
      </el-table-column>
      <el-table-column prop="quantity" label="数量" />
      <el-table-column prop="price" label="单价" />
      <el-table-column prop="totalAmount" label="合计" />
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
      <el-table-column label="操作" width="140">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="handleEdit(row)" v-text="'编辑'" />
          <el-button link size="small" type="danger" @click="handleDelete(row)" v-text="'删除'" />
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination">
      <el-pagination
        v-model:current-page="pageIndex"
        v-model:page-size="pageSize"
        :page-sizes="ConstDictionary.CHILD_TABLE_PAGE_SIZES"
        :total="total"
        layout="total, sizes, prev, pager, next, jumper"
        @current-page-change="handleCurrentPageChange"
        @page-size-change="handlePageSizeChange"
      />
    </div>

    <InboundItemEdit
      v-model="editVisible"
      :item="editing"
      :orderId="props.orderId"
      @saved="handleSaved"
    />
  </div>
</template>
