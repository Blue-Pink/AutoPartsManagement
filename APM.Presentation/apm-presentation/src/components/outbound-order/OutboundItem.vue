<script setup lang="ts">
import { ref, watch } from 'vue'
import type { OutboundItem } from '@/interfaces/Entities'
import { ElMessage, ElMessageBox } from 'element-plus'
import OutboundItemEdit from '@/components/outbound-order/OutboundItemEdit.vue'
import UsualEntityService from '@/services/UsualEntityService'
import { ConstDictionary } from '@/utils/const-dictionary'
import { ConvertDateTime } from '@/utils/converter'
import { _initialOutboundItem } from '@/utils/initialEntity'

const props = defineProps<{ orderId: string | null }>()
const pageIndex = ref(1)
const pageSize = ref(5)
const orderBy = ref<string>('')
const descending = ref<boolean>(false)
const total = ref(0)
const items = ref<OutboundItem[]>([])
const editVisible = ref(false)
const editing = ref<OutboundItem>({ ..._initialOutboundItem })
const selected = ref<OutboundItem[]>([])
const emit = defineEmits(['saved'])

const load = async () => {
  if (!props.orderId) return
  try {
    if (props.orderId && props.orderId !== ConstDictionary.EMPTY_GUID) {
      const res = await UsualEntityService.GetChildrenDataSet<OutboundItem>(
        'OutboundOrder',
        'OutboundItem',
        props.orderId,
        pageIndex.value,
        pageSize.value,
        orderBy.value,
        descending.value,
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

const handleAdd = () => {
  editing.value = {
    id: ConstDictionary.EMPTY_GUID,
    createdAt: ConstDictionary.CURRENT_DATETIME,
    modifiedAt: ConstDictionary.CURRENT_DATETIME,
    operatorUserId: ConstDictionary.EMPTY_GUID,
    outboundOrderId: null,
    partId: null,
    quantity: 0,
    price: 0,
    totalAmount: 0,
  }
  editVisible.value = true
}

const handleSelectionChange = (sel: OutboundItem[]) => {
  selected.value = sel
}

const handleEdit = (row: OutboundItem) => {
  editing.value = { ...row }
  editVisible.value = true
}

const handleDelete = async (row: OutboundItem) => {
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
    await UsualEntityService.Delete('OutboundItem', ids)
    ElMessage.success('删除成功')
    load()
    emit('saved')
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error('删除失败')
  }
}

const handleBatchDelete = async () => {
  if (selected.value.length === 0) {
    ElMessage.warning('请先选择要删除的出库单明细')
    return
  }
  try {
    await ElMessageBox.confirm(
      `确定要删除选中的 ${selected.value.length} 个出库单明细吗？`,
      '警告',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )
    const ids = selected.value.map((s) => s.id!)
    await UsualEntityService.Delete('OutboundItem', ids)
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
        <h2>出库单明细管理</h2>
      </div>
      <div class="toolbar-right">
        <el-button type="primary" @click="handleAdd">添加明细</el-button>
        <el-button type="danger" @click="handleBatchDelete">删除明细</el-button>
      </div>
    </div>
    <el-table
      :data="items"
      class="apm-table"
      @selection-change="handleSelectionChange"
      @sort-change="handleSortChange"
    >
      <el-table-column type="selection" width="55" />
      <el-table-column label="配件" sortable="custom" prop="part">
        <template #default="{ row }">{{ `${row.part?.partName} ( ${row.part?.model} )` }}</template>
      </el-table-column>
      <el-table-column prop="quantity" label="数量" sortable="custom" />
      <el-table-column prop="price" label="单价" sortable="custom" />
      <el-table-column prop="totalAmount" label="合计" sortable="custom" />
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

    <OutboundItemEdit
      v-model="editVisible"
      :item="editing"
      :orderId="props.orderId"
      @saved="handleSaved"
    />
  </div>
</template>
