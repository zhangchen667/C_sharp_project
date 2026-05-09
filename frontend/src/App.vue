<template>
  <div id="app">
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
import { useUserStore } from './stores/user'

const userStore = useUserStore()

const logout = () => {
  userStore.logout()
  location.href = '/'
}
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

#app {
  min-height: 100vh;
  background: #f5f5f5;
  width: 100%;
}

.el-container {
  width: 100%;
}

.el-header {
  background: #fff;
  border-bottom: 1px solid #eee;
  padding: 0;
  width: 100%;
  height: auto !important;
}

.header-content {
  display: flex;
  align-items: center;
  width: 100%;
}

.nav-menu {
  flex: 1;
  border-bottom: none;
}

.nav-right {
  margin-left: auto;
  padding-right: 20px;
  display: flex;
  align-items: center;
}

.el-main {
  width: 100%;
}

.el-footer {
  text-align: center;
  color: #999;
  padding: 20px 0;
  width: 100%;
}

.text-center {
  text-align: center;
}
</style>
