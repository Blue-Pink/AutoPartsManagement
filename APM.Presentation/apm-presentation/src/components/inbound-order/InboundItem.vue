<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import InboundOrderService from '@/services/InboundOrderService'
import type { InboundItem } from '@/interfaces/DTOEntities'
import { ElMessage, ElMessageBox } from 'element-plus'
import InboundItemEdit from '@/components/inbound-order/InboundItemEdit.vue'
import UsualEntityService from '@/services/UsualEntityService'

const props = defineProps<{ orderId: string | null }>()
const emit = defineEmits(['saved'])

const items = ref<InboundItem[]>([])
const editVisible = ref(false)
const editing = ref<InboundItem | null>(null)

const load = async () => {
  if (!props.orderId) return
  try {
    const res = await UsualEntityService.GetChildrenDataSetQuery(
      'InboundOrder',
      'InboundItem',
      props.orderId,
    )
    items.value = res.dataList || []
  } catch (e) {
    console.error(e)
  } finally {
  }
}

const handleAdd = () => {
  editing.value = null
  editVisible.value = true
}
const handleEdit = (row: InboundItem) => {
  editing.value = { ...row }
  editVisible.value = true
}

const handleDelete = async (row: InboundItem) => {
  try {
    await ElMessageBox.confirm('确定要删除该明细吗？', '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const ids = [row.id!]
    await UsualEntityService.Delete('InboundItem', ids)
    ElMessage.success('删除成功')
    load()
    emit('saved')
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
      <el-button type="primary" @click="handleAdd">添加明细</el-button>
    </div>
    <el-table :data="items" class="apm-table">
      <el-table-column prop="partId" label="配件" />
      <el-table-column prop="quantity" label="数量" />
      <el-table-column prop="price" label="单价" />
      <el-table-column prop="totalAmount" label="合计" />
      <el-table-column label="操作" width="140">
        <template #default="{ row }">
          <el-button link size="small" @click="handleEdit(row)">编辑</el-button>
          <el-button link size="small" type="danger" @click="handleDelete(row)">删除</el-button>
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
