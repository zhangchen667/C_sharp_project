<template>
  <div class="home">
    <h1 class="home-title">欢迎来到我的个人空间</h1>

    <p class="home-subtitle">
      记录生活，分享技术，留住每一个温暖的瞬间。
    </p>

    <el-row :gutter="24" class="cards">
      <el-col :lg="8" :md="12" :sm="24" v-for="item in navItems" :key="item.title">
        <el-card
          class="nav-card"
          shadow="hover"
          @click="$router.push(item.path)"
          :tabindex="0"
          @keydown.enter="$router.push(item.path)"
        >
          <div class="card-content">
            <div class="icon-wrapper" :style="{ background: item.bgColor }">
              <el-icon :size="28" :color="item.color">
                <component :is="item.icon" />
              </el-icon>
            </div>
            <h3>{{ item.title }}</h3>
            <p>{{ item.desc }}</p>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-card class="intro-card">
      <template #header>
        <span class="card-header-title">技术栈</span>
      </template>
      <div class="author-note">
        本站基于 ASP.NET Core Web API + Vue 3 构建，采用前后端分离架构，
        数据库使用 MySQL，身份认证基于 JWT。
      </div>
      <el-row :gutter="16" class="tag-list">
        <el-col :lg="6" :md="8" :sm="12" v-for="tech in techStack" :key="tech">
          <el-tag>{{ tech }}</el-tag>
        </el-col>
      </el-row>
    </el-card>
  </div>
</template>

<script setup>
import { Document, Picture, ChatDotSquare } from '@element-plus/icons-vue'

const navItems = [
  {
    title: '博文',
    desc: '技术文章和生活随笔',
    path: '/posts',
    icon: Document,
    color: '#B85C4A',
    bgColor: 'rgba(212, 124, 107, 0.1)'
  },
  {
    title: '相册',
    desc: '记录美好瞬间',
    path: '/albums',
    icon: Picture,
    color: '#529B2E',
    bgColor: 'rgba(103, 194, 58, 0.1)'
  },
  {
    title: '留言',
    desc: '欢迎交流互动',
    path: '/comments',
    icon: ChatDotSquare,
    color: '#B8822E',
    bgColor: 'rgba(230, 162, 60, 0.1)'
  }
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
  position: relative;
  z-index: 1;
}

.home-title {
  font-family: var(--font-family-serif);
  font-size: var(--font-size-3xl);
  font-weight: 600;
  text-align: center;
  margin-bottom: var(--spacing-sm);
  color: var(--text-primary);
}

.home-subtitle {
  text-align: center;
  color: var(--text-secondary);
  font-size: var(--font-size-lg);
  margin-bottom: var(--spacing-2xl);
}

.cards {
  margin-bottom: var(--spacing-xl);
}

.nav-card {
  cursor: pointer;
  text-align: center;
  height: 100%;
}

.nav-card:hover {
  transform: translateY(-4px);
}

.icon-wrapper {
  width: 64px;
  height: 64px;
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto var(--spacing-md);
}

.card-content {
  padding: var(--spacing-md) 0;
}

.card-content h3 {
  font-family: var(--font-family-serif);
  margin: 0 0 var(--spacing-sm);
  font-size: var(--font-size-xl);
  color: var(--text-primary);
}

.card-content p {
  color: var(--text-secondary);
  margin: 0;
  font-size: var(--font-size-sm);
}

.intro-card {
  margin-top: var(--spacing-xl);
}

.card-header-title {
  font-family: var(--font-family-serif);
  font-weight: 600;
  font-size: var(--font-size-lg);
}

.tag-list {
  row-gap: 12px;
}
</style>
