<template>
  <div class="auth-page">
    <el-card class="auth-card">
      <template #header>
        <div class="card-header">
          <h2>用户登录</h2>
        </div>
      </template>
      <el-form ref="formRef" :model="form" label-width="80px">
        <el-form-item label="邮箱" prop="email"
          :rules="[{ required: true, message: '请输入邮箱' }, { type: 'email', message: '邮箱格式不正确' }]">
          <el-input v-model="form.email" placeholder="请输入邮箱" />
        </el-form-item>
        <el-form-item label="密码" prop="password"
          :rules="[{ required: true, message: '请输入密码' }]">
          <el-input v-model="form.password" type="password" placeholder="请输入密码" show-password />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleLogin" class="submit-btn">登录</el-button>
        </el-form-item>
        <div class="tips">
          还没有账号？<el-link type="primary" @click="$router.push('/register')">立即注册</el-link>
        </div>
      </el-form>
    </el-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { authApi } from '../api'
import { useUserStore } from '../stores/user'

const router = useRouter()
const userStore = useUserStore()
const formRef = ref()
const form = ref({
  email: '',
  password: ''
})

const handleLogin = async () => {
  await formRef.value?.validate()
  try {
    const res = await authApi.login(form.value)
    if (res.success) {
      ElMessage.success('登录成功')
      userStore.login(res.token, res.user)
      router.push('/')
    } else {
      ElMessage.error(res.message || '登录失败')
    }
  } catch (e) {
    ElMessage.error(e.response?.data?.message || '登录失败')
  }
}
</script>

<style scoped>
.auth-page {
  display: flex;
  justify-content: center;
  align-items: flex-start;
  padding: var(--spacing-2xl) var(--spacing-lg);
}

.auth-card {
  width: 100%;
  max-width: 450px;
}

.card-header {
  text-align: center;
}

.card-header h2 {
  font-family: var(--font-family-serif);
  font-size: var(--font-size-xl);
  margin: 0;
  color: var(--text-primary);
}

.submit-btn {
  width: 100%;
}

.tips {
  text-align: center;
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}
</style>
