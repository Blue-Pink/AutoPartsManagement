<script lang="ts">
import UserRoleService from "@/services/UserRoleService";

export default {
    name: "Login",
    setup() {
    },
    methods: {
        login() {
            UserRoleService.UserLogin("Administrator", "Administrator@01").then(res => {
                console.log(res)
            })

        }
    },
}
</script>

<template>
    <div>
        <el-button type="primary" @click="login">Primary</el-button>
    </div>
</template>