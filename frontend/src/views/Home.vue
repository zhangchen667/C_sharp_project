<template>
  <div class="home">
    <h1 class="home-title">欢迎来到我的个人空间</h1>

    <el-row :gutter="20" class="cards">
      <el-col :span="8" v-for="item in navItems" :key="item.title">
        <el-card class="nav-card" shadow="hover" @click="$router.push(item.path)">
          <div class="card-content">
            <el-icon :size="50" :color="item.color">
              <component :is="item.icon" />
            </el-icon>
            <h3>{{ item.title }}</h3>
            <p>{{ item.desc }}</p>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-card class="intro-card">
      <template #header>
        <span>技术栈</span>
      </template>
      <el-row :gutter="20">
        <el-col :span="6" v-for="tech in techStack" :key="tech">
          <el-tag>{{ tech }}</el-tag>
        </el-col>
      </el-row>
    </el-card>
  </div>
</template>

<script setup>
import { Document, Picture, ChatDotSquare, Management } from '@element-plus/icons-vue'

const navItems = [
  { title: '博文', desc: '技术文章和生活随笔', path: '/posts', icon: Document, color: '#409eff' },
  { title: '相册', desc: '记录美好瞬间', path: '/albums', icon: Picture, color: '#67c23a' },
  { title: '留言', desc: '欢迎交流互动', path: '/comments', icon: ChatDotSquare, color: '#e6a23c' }
]

const techStack = [
  'ASP.NET Core Web API',
  'Vue 3',
  'Element Plus',
  'MySQL',
  'JWT 认证',
  '前后端分离'
]
</script>

<style scoped>
.home {
  padding: 20px;
}

.home-title {
  font-size: 28px;
  font-weight: 500;
  text-align: center;
  margin-bottom: 40px;
  color: #303133;
}

.cards {
  margin-bottom: 30px;
}

.nav-card {
  cursor: pointer;
  text-align: center;
  transition: transform 0.3s;
}

.nav-card:hover {
  transform: translateY(-5px);
}

.card-content {
  padding: 20px 0;
}

.card-content h3 {
  margin: 15px 0 10px;
  font-size: 20px;
}

.card-content p {
  color: #666;
  margin: 0;
}

.intro-card {
  margin-top: 30px;
}
</style>
