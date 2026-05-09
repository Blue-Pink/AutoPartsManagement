<template>
  <div class="apm-container" style="height: 90vh">
    <!-- 顶部数据卡片 -->
    <el-row :gutter="24">
      <el-col :span="6">
        <el-card class="kpi-card inbound">
          <div class="label">今日入库金额</div>
          <div class="value">
            <el-icon><Wallet /></el-icon>
            {{ stats.todayInboundTotalAmount }}
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card class="kpi-card inbound">
          <div class="label">今日入库总量</div>
          <div class="value">
            <el-icon><ShoppingCartFull /></el-icon>
            {{ stats.todayInboundQuantity }}
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card class="kpi-card outbound">
          <div class="label">今日出库金额</div>
          <div class="value">
            <el-icon><Money /></el-icon>
            {{ stats.todayOutboundTotalAmount }}
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card class="kpi-card outbound">
          <div class="label">今日出库总量</div>
          <div class="value">
            <el-icon><SoldOut /></el-icon>
            {{ stats.todayOutboundQuantity }}
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 图表区域 -->
    <el-row :gutter="24" style="margin-top: 20px">
      <el-col :span="8">
        <el-card header="今日配件分类收入分布">
          <div ref="pieChartRef" style="height: 350px"></div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card header="今日配件分类支出分布">
          <div ref="pieChartRef2" style="height: 350px"></div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card header="近七天收支趋势">
          <div ref="lineChartRef" style="height: 350px"></div>
        </el-card>
      </el-col>
    </el-row>
    <el-row :gutter="24" style="margin-top: 20px">
      <el-col :span="8">
        <el-card header="今日配件分类库存入库">
          <div ref="pieChartRef3" style="height: 350px"></div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card header="今日配件分类库存出库">
          <div ref="pieChartRef4" style="height: 350px"></div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card header="近七天货量趋势">
          <div ref="lineChartRef2" style="height: 350px"></div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, shallowRef, reactive } from 'vue'
import * as echarts from 'echarts'
import DashboardService from '@/services/DashboardService'
import type { DashboardDTO, LineChartData, PieChartData } from '@/interfaces/Entities'

const lineChartRef = ref<HTMLElement>()
const lineChartRef2 = ref<HTMLElement>()
const pieChartRef = ref<HTMLElement>()
const pieChartRef2 = ref<HTMLElement>()
const pieChartRef3 = ref<HTMLElement>()
const pieChartRef4 = ref<HTMLElement>()
const stats = reactive<DashboardDTO>({} as DashboardDTO)

onMounted(async () => {
  const res = await DashboardService.GetStockFlowDash()
  const data = res.data
  if (!data) return

  Object.assign(stats, data)

  const lineChart = echarts.init(lineChartRef.value!)
  lineChart.setOption({
    tooltip: { trigger: 'axis' },
    xAxis: { type: 'category', data: data.lastSevenDaysTrend.map((x: LineChartData) => x.date) },
    yAxis: { type: 'value' },
    series: [
      {
        name: '入库',
        data: data.lastSevenDaysTrend.map((x: LineChartData) => x.inboundTotalAmount),
        type: 'line',
        smooth: true,
        color: '#409EFF',
      },
      {
        name: '出库',
        data: data.lastSevenDaysTrend.map((x: LineChartData) => x.outboundTotalAmount),
        type: 'line',
        smooth: true,
        color: '#F56C6C',
      },
    ],
  })

  const lineChart2 = echarts.init(lineChartRef2.value!)
  lineChart2.setOption({
    tooltip: { trigger: 'axis' },
    xAxis: { type: 'category', data: data.lastSevenDaysTrend.map((x: LineChartData) => x.date) },
    yAxis: { type: 'value' },
    series: [
      {
        name: '入库',
        data: data.lastSevenDaysTrend.map((x: LineChartData) => x.inboundQuantity),
        type: 'line',
        smooth: true,
        color: '#409EFF',
      },
      {
        name: '出库',
        data: data.lastSevenDaysTrend.map((x: LineChartData) => x.outboundQuantity),
        type: 'line',
        smooth: true,
        color: '#F56C6C',
      },
    ],
  })

  // 初始化饼图
  const pieChart = echarts.init(pieChartRef.value!)
  pieChart.setOption({
    tooltip: { trigger: 'item' },
    series: [
      {
        type: 'pie',
        radius: '60%',
        data: data.categoryDistribution.map((x: PieChartData) => ({
          name: x.categoryName,
          value: x.inboundTotalAmount,
        })),
        emphasis: {
          itemStyle: { shadowBlur: 10, shadowOffsetX: 0, shadowColor: 'rgba(0, 0, 0, 0.5)' },
        },
      },
    ],
  })

  // 初始化第二个饼图
  const pieChart2 = echarts.init(pieChartRef2.value!)
  pieChart2.setOption({
    tooltip: { trigger: 'item' },
    series: [
      {
        type: 'pie',
        radius: '60%',
        data: data.categoryDistribution.map((x: PieChartData) => ({
          name: x.categoryName,
          value: x.outboundTotalAmount,
        })),
        emphasis: {
          itemStyle: { shadowBlur: 10, shadowOffsetX: 0, shadowColor: 'rgba(0, 0, 0, 0.5)' },
        },
      },
    ],
  })

  const pieChart3 = echarts.init(pieChartRef3.value!)
  pieChart3.setOption({
    tooltip: { trigger: 'item' },
    series: [
      {
        type: 'pie',
        radius: '60%',
        data: data.categoryDistribution.map((x: PieChartData) => ({
          name: x.categoryName,
          value: x.inboundQuantity,
        })),
        emphasis: {
          itemStyle: { shadowBlur: 10, shadowOffsetX: 0, shadowColor: 'rgba(0, 0, 0, 0.5)' },
        },
      },
    ],
  })

  const pieChart4 = echarts.init(pieChartRef4.value!)
  pieChart4.setOption({
    tooltip: { trigger: 'item' },
    series: [
      {
        type: 'pie',
        radius: '60%',
        data: data.categoryDistribution.map((x: PieChartData) => ({
          name: x.categoryName,
          value: -x.outboundQuantity,
        })),
        emphasis: {
          itemStyle: { shadowBlur: 10, shadowOffsetX: 0, shadowColor: 'rgba(0, 0, 0, 0.5)' },
        },
      },
    ],
  })
})
</script>

<style scoped>
.kpi-card {
  text-align: center;
  padding: 10px;
}
.kpi-card .label {
  font-size: 14px;
  color: #909399;
}
.kpi-card .value {
  font-size: 24px;
  font-weight: bold;
  margin-top: 10px;
}
.inbound .value {
  color: #409eff;
}
.outbound .value {
  color: #f56c6c;
}
</style>
