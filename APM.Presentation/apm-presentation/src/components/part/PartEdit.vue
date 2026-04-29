<script setup lang="ts">
import { ref, watch, onMounted, reactive } from 'vue'
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from 'element-plus'
import PartService from '@/services/PartService'
import type { Category, Part, Unit } from '@/interfaces/DTOEntities'
import UsualEntityService from '@/services/UsualEntityService'
import type { UsualApiData } from '@/interfaces/HttpReponse'
import { _initialPart } from '@/utils/initialEntity'

const props = defineProps<{
  modelValue: boolean
  part: Part | null
}>()
const emit = defineEmits(['update:modelValue', 'saved'])
const visible = ref(props.modelValue)
const userFormRef = ref<FormInstance>()
const part = ref<Part>({ ..._initialPart })
const categories = ref<Category[]>([])
const units = ref<Unit[]>([])
const rules = reactive<FormRules>({
  partName: [
    { required: true, message: '请输入名称', trigger: 'blur' },
    { min: 2, max: 50, message: '名称长度必须在 2-50 之间', trigger: 'blur' },
  ],
  oeCode: [
    { required: true, message: '请输入OE代码', trigger: 'blur' },
    { min: 2, max: 50, message: 'OE代码长度必须在 2-50 之间', trigger: 'blur' },
  ],
  model: [
    { required: true, message: '请输入型号', trigger: 'blur' },
    { min: 2, max: 50, message: '型号长度必须在 2-50 之间', trigger: 'blur' },
  ],
  brand: [
    { required: true, message: '请输入品牌', trigger: 'blur' },
    { min: 2, max: 50, message: '品牌长度必须在 2-50 之间', trigger: 'blur' },
  ],
  categories: [{ required: true, message: '请选择分类', trigger: 'change' }],
  units: [{ required: true, message: '请选择单位', trigger: 'change' }],
  costPrice: [{ required: true, message: '请输入成本价', trigger: 'blur' }],
  sellingPrice: [{ required: true, message: '请输入售价', trigger: 'blur' }],
  minStock: [{ required: true, message: '请输入最小库存', trigger: 'blur' }],
  maxStock: [{ required: true, message: '请输入最大库存', trigger: 'blur' }],
})

const loadOptions = async () => {
  try {
    const c = await PartService.GetCategories()
    categories.value = c.dataList || []
    part.value.categoryId = part.value.categoryId || categories.value[0]?.id || null
    const u = await PartService.GetUnits()
    units.value = u.dataList || []
    part.value.unitId = part.value.unitId || units.value[0]?.id || null
  } catch (e) {
    console.error(e)
  }
}

const close = () => emit('update:modelValue', false)

const handleSave = async () => {
  try {
    await userFormRef.value?.validate()
    await PartService.EditPart(part.value)
    ElMessage.success('保存成功')
    emit('saved')
    close()
  } catch (error) {
    console.error('EditPart failed', error)
  }
}

onMounted(loadOptions)

watch(
  () => props.modelValue,
  (val: boolean) => {
    visible.value = val
  },
)

watch(visible, (val: boolean) => {
  if (part.value && !part.value.id) {
    part.value.categoryId = part.value.categoryId || categories.value[0]?.id || null
    part.value.unitId = part.value.unitId || units.value[0]?.id || null
  }
  emit('update:modelValue', val)
})

watch(
  () => props.part,
  (p: Part | null) => {
    if (p && p.id) {
      UsualEntityService.Get<Part>('Part', p.id)
        .then((res: UsualApiData<Part>) => {
          if (res.data) {
            part.value = res.data as Part
          }
        })
        .catch((error) => {
          console.error('加载配件数据失败', error)
        })
    } else {
      part.value = { ..._initialPart }
    }
  },
  { immediate: true },
)
</script>

<template>
  <el-dialog
    v-model="visible"
    :title="part.id ? '配件编辑' : '配件新增'"
    width="35vw"
    @close="close"
  >
    <el-form :model="part" :rules="rules" label-width="100px" ref="userFormRef">
      <el-form-item label="名称" prop="partName">
        <el-input v-model="part.partName" />
      </el-form-item>
      <el-form-item label="OE代码" prop="oeCode">
        <el-input v-model="part.oeCode" />
      </el-form-item>
      <el-form-item label="型号" prop="model">
        <el-input v-model="part.model" />
      </el-form-item>
      <el-form-item label="品牌" prop="brand">
        <el-input v-model="part.brand" />
      </el-form-item>
      <el-form-item label="分类">
        <el-select v-model="part.categoryId" placeholder="请选择分类">
          <el-option v-for="c in categories" :key="c.id" :label="c.name" :value="c.id" />
        </el-select>
      </el-form-item>
      <el-form-item label="单位">
        <el-select v-model="part.unitId" placeholder="请选择单位">
          <el-option v-for="u in units" :key="u.id" :label="u.name" :value="u.id" />
        </el-select>
      </el-form-item>
      <el-form-item label="成本价">
        <el-input-number v-model="part.costPrice" :min="0.01" />
      </el-form-item>
      <el-form-item label="售价">
        <el-input-number v-model="part.sellingPrice" :min="0.01" />
      </el-form-item>
      <el-form-item label="最小库存">
        <el-input-number v-model="part.minStock" :min="1" />
      </el-form-item>
      <el-form-item label="最大库存">
        <el-input-number v-model="part.maxStock" :min="1" />
      </el-form-item>
      <el-form-item label="备注">
        <el-input type="textarea" v-model="part.remark" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="close">取消</el-button>
      <el-button type="primary" @click="handleSave">保存</el-button>
    </template>
  </el-dialog>
</template>

<style scoped></style>
