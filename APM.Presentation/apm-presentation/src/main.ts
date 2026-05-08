import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import '@/styles/index.css'
import App from './App.vue'
import router from './router'
import 'element-plus/theme-chalk/dark/css-vars.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import zhCn from 'element-plus/es/locale/lang/zh-cn'
import locale from 'dayjs/locale/zh-cn'
import { dayjs } from 'element-plus'

dayjs.locale(locale)
const app = createApp(App)

for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
    app.component(key, component)
}

localStorage.getItem('apm-ui-mode') === 'dark' && document.documentElement.classList.add('dark')

app.use(createPinia())
app.use(router)
app.use(ElementPlus, {
    locale: zhCn,
})
app.mount('#app')
