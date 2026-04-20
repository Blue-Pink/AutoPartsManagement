import { ref } from 'vue'
import { defineStore } from 'pinia'

export const userAuthStore = defineStore('auth', () => {
    const loginState = ref(false)

    function setLoginState(value: boolean) {
        loginState.value = value
    }

    return {
        loginState,
        setLoginState,
    }
})
