<template>
  <div id="app">
    <!-- 全局纸张纹理装饰 -->
    <div class="paper-texture"></div>
    <!-- 装饰性 Blob 背景 -->
    <div class="decoration-blob decoration-blob-1"></div>
    <div class="decoration-blob decoration-blob-2"></div>

    <el-container>
      <el-header>
        <div class="header-content">
          <el-menu
            :default-active="$route.path"
            mode="horizontal"
            router
            class="nav-menu"
          >
            <el-menu-item index="/">首页</el-menu-item>
            <el-menu-item index="/posts">博文</el-menu-item>
            <el-menu-item index="/albums">相册</el-menu-item>
            <el-menu-item index="/comments">留言</el-menu-item>
            <el-menu-item v-if="userStore.isAdmin" index="/admin">管理后台</el-menu-item>
          </el-menu>
          <div class="nav-right">
            <ThemeToggle />
            <template v-if="userStore.isLoggedIn">
              <el-button @click="logout" type="text">退出登录</el-button>
            </template>
            <template v-else>
              <el-button @click="$router.push('/login')" type="text">登录</el-button>
              <el-button @click="$router.push('/register')" type="text">注册</el-button>
            </template>
          </div>
        </div>
      </el-header>
      <el-main>
        <router-view />
      </el-main>
      <el-footer class="text-center">
        &copy; 2026 我的个人空间
      </el-footer>
    </el-container>
  </div>
</template>

<script setup>
import { onBeforeMount } from 'vue'
import { useUserStore } from './stores/user'
import { useThemeStore } from './stores/theme'
import ThemeToggle from './components/ThemeToggle.vue'

const userStore = useUserStore()
const themeStore = useThemeStore()

// 应用启动时初始化主题
onBeforeMount(() => {
  themeStore.initTheme()
})

const logout = () => {
  userStore.logout()
  location.href = '/'
}
</script>

<style scoped>
#app {
  min-height: 100vh;
  background-color: var(--bg-primary);
  width: 100%;
  position: relative;
  overflow-x: hidden;
}

.el-container {
  width: 100%;
  position: relative;
  z-index: 1;
}

.el-header {
  background-color: var(--bg-primary);
  border-bottom: 1px solid var(--border-light);
  padding: 0;
  width: 100%;
  height: auto !important;
  backdrop-filter: blur(8px);
  position: sticky;
  top: 0;
  z-index: 100;
}

.header-content {
  display: flex;
  align-items: center;
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 var(--spacing-lg);
}

.nav-menu {
  flex: 1;
}

.nav-right {
  margin-left: auto;
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
}

.el-main {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  padding: var(--spacing-xl) var(--spacing-lg);
}

.el-footer {
  text-align: center;
  color: var(--text-muted);
  padding: var(--spacing-xl) 0;
  width: 100%;
  border-top: 1px solid var(--border-light);
  margin-top: var(--spacing-2xl);
}

.text-center {
  text-align: center;
}
</style>
