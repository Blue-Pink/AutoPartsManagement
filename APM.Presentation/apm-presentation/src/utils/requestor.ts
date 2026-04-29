import axios from 'axios';
import type { AxiosInstance, InternalAxiosRequestConfig, AxiosResponse, AxiosError } from 'axios';

import { ElLoading, ElMessage } from 'element-plus';
import { ref } from 'vue';

const loadingInst = ref({} as ReturnType<typeof ElLoading.service>);
// 创建 Axios 实例
const service: AxiosInstance = axios.create({
    // 根据你后端 .NET 项目的启动端口修改
    baseURL: 'https://172.23.99.139:8081/api/',
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json'
    }
});

// 请求拦截器
service.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        loadingInst.value = ElLoading.service({ lock: true, text: '执行中...' })

        // 从本地存储获取 Token
        const token = localStorage.getItem('token');
        if (token && config.headers) {
            // 在请求头中注入 JWT Token
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error: AxiosError) => {
        if (loadingInst.value && loadingInst.value.close) {
            loadingInst.value.close();
        }
        return Promise.reject(error);
    }
);

// 响应拦截器
service.interceptors.response.use(
    (response: AxiosResponse) => {
        if (response.data && response.data.stateCode) {
            if (loadingInst.value && loadingInst.value.close) {
                loadingInst.value.close();
            }
            return response.data;
        }
        else {
            ElMessage.error(response.data.message || '执行失败');
            if (loadingInst.value && loadingInst.value.close) {
                loadingInst.value.close();
            }
            return Promise.reject(new Error(response.data.message || '执行失败'));
        }

    },
    (error: AxiosError) => {
        if (loadingInst.value && loadingInst.value.close) {
            loadingInst.value.close();
        }
        // 统一处理错误响应
        let message = '';
        if (error.response) {
            const status = error.response.status;
            switch (status) {
                case 401:
                    if (localStorage.getItem('token'))
                        ElMessage.error('用户凭证过期, 请重新登录');
                    // 可以在这里处理退出登录逻辑，如清除 token 并跳转到登录页
                    localStorage.removeItem('token');
                    break;
                case 403:
                    ElMessage.warning('拒绝访问: 您没有对应操作权限');
                    break;
                case 404:
                    ElMessage.warning('请求资源不存在');
                    break;
                case 500:
                    ElMessage.error('服务器内部错误, 请联系管理员');
                    break;
                default:
                    // 尝试获取后端返回的验证错误信息（如之前的 PasswordHash 报错）
                    const data: any = error.response.data;
                    message = data?.message || data?.title || '系统异常';
            }
        } else {
            message = '网络连接超时或服务器无响应';
        }

        return Promise.reject(error);
    }
);

export default service;