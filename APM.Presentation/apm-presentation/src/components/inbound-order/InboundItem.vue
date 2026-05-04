<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import type { InboundItem } from '@/interfaces/DTOEntities'
import { ElMessage, ElMessageBox } from 'element-plus'
import InboundItemEdit from '@/components/inbound-order/InboundItemEdit.vue'
import UsualEntityService from '@/services/UsualEntityService'
import { ConstDictionary } from '@/utils/const-dictionary'

const props = defineProps<{ orderId: string | null }>()
const emit = defineEmits(['saved'])

const items = ref<InboundItem[]>([])
const editVisible = ref(false)
const editing = ref<InboundItem | null>(null)
const selected = ref<InboundItem[]>([])

const load = async () => {
  if (!props.orderId) return
  try {
    if (props.orderId && props.orderId !== ConstDictionary.EMPTY_GUID) {
      const res = await UsualEntityService.GetChildrenDataSetQuery<InboundItem>(
        'InboundOrder',
        'InboundItem',
        props.orderId,
        1,
        10,
      )
      items.value = res.dataList || []
    }
  } catch (e) {
    console.error(e)
  }
}

const handleAdd = () => {
  editing.value = null
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

onMounted(() => {
  load()
})

watch(
  () => props.orderId,
  () => {
    load()
  },
)
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
    <el-table :data="items" class="apm-table" @selection-change="handleSelectionChange">
      <el-table-column type="selection" width="55" />
      <el-table-column label="配件">
        <template #default="{ row }">{{
          `[${row.part?.model}] - [${row.part?.partName}]`
        }}</template>
      </el-table-column>
      <el-table-column prop="quantity" label="数量" />
      <el-table-column prop="price" label="单价" />
      <el-table-column prop="totalAmount" label="合计" />
      <el-table-column label="操作" width="140">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="handleEdit(row)" v-text="'编辑'" />
          <el-button link size="small" type="danger" @click="handleDelete(row)" v-text="'删除'" />
        </template>
      </el-table-column>
    </el-table>

    <InboundItemEdit
      v-model="editVisible"
      :item="editing"
      :orderId="props.orderId"
      @saved="handleSaved"
    />
  </div>
</template>
