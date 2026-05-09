<template>
  <div class="register">
    <el-card class="register-card">
      <template #header>
        <div class="card-header">
          <span>用户注册</span>
        </div>
      </template>
      <el-form ref="formRef" :model="form" label-width="80px">
        <el-form-item label="邮箱" prop="email"
          :rules="[{ required: true, message: '请输入邮箱' }, { type: 'email', message: '邮箱格式不正确' }]">
          <el-input v-model="form.email" placeholder="请输入邮箱" />
        </el-form-item>
        <el-form-item label="昵称" prop="nickname"
          :rules="[{ required: true, message: '请输入昵称' }, { min: 2, max: 20, message: '昵称长度2-20字符' }]">
          <el-input v-model="form.nickname" placeholder="请输入昵称" />
        </el-form-item>
        <el-form-item label="密码" prop="password"
          :rules="[{ required: true, message: '请输入密码' }, { min: 6, message: '密码至少6位' }]">
          <el-input v-model="form.password" type="password" placeholder="请输入密码" show-password />
          <div class="password-hint">密码要求：至少6位，包含大写字母、小写字母、数字和特殊字符</div>
        </el-form-item>
        <el-form-item label="确认密码" prop="confirmPassword"
          :rules="[{ required: true, message: '请确认密码' }]">
          <el-input v-model="form.confirmPassword" type="password" placeholder="请再次输入密码" show-password />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleRegister" style="width: 100%">注册</el-button>
        </el-form-item>
        <div class="tips">
          已有账号？<el-link type="primary" @click="$router.push('/login')">立即登录</el-link>
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
  nickname: '',
  password: '',
  confirmPassword: ''
})

const handleRegister = async () => {
  await formRef.value?.validate()
  if (form.value.password !== form.value.confirmPassword) {
    ElMessage.error('两次密码不一致')
    return
  }
  try {
    const res = await authApi.register(form.value)
    if (res.success) {
      ElMessage.success('注册成功')
      userStore.login(res.token, res.user)
      router.push('/')
    } else {
      ElMessage.error(res.message || '注册失败')
    }
  } catch (e) {
    const errors = e.response?.data
    if (errors) {
      const messages = Object.values(errors).flat()
      ElMessage.error(messages[0] || '注册失败')
    } else {
      ElMessage.error('注册失败')
    }
  }
}
</script>

<style scoped>
.register {
  display: flex;
  justify-content: center;
  padding: 50px 0;
}

.register-card {
  width: 450px;
}

.card-header {
  text-align: center;
  font-size: 20px;
  font-weight: bold;
}

.tips {
  text-align: center;
  color: #666;
}

.password-hint {
  font-size: 12px;
  color: #999;
  margin-top: 4px;
}
</style>
