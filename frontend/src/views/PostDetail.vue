<template>
  <div class="post-detail">
    <el-button class="back-btn" @click="$router.back()">
      <el-icon><ArrowLeft /></el-icon>
      返回列表
    </el-button>
    <el-card class="detail-card">
      <h1 class="post-title">{{ post.title }}</h1>
      <div class="post-meta">
        <el-tag size="small">{{ post.categoryName }}</el-tag>
        <span class="author">{{ post.authorName || '匿名' }}</span>
        <span class="date">{{ formatDate(post.createdAt) }}</span>
      </div>
      <div class="post-content">{{ post.content }}</div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ArrowLeft } from '@element-plus/icons-vue'
import { postsApi } from '../api'

const route = useRoute()
const post = ref({ title: '', content: '' })

const fetchPost = async () => {
  const res = await postsApi.getDetail(route.params.id)
  post.value = res
}

const formatDate = (date) => {
  return new Date(date).toLocaleString()
}

onMounted(fetchPost)
</script>

<style scoped>
.post-detail {
  position: relative;
  z-index: 1;
}

.back-btn {
  margin-bottom: var(--spacing-lg);
}

.detail-card {
  margin-top: var(--spacing-lg);
}

.post-title {
  font-family: var(--font-family-serif);
  font-size: var(--font-size-2xl);
  margin: 0 0 var(--spacing-lg);
  text-align: center;
  color: var(--text-primary);
}

.post-meta {
  text-align: center;
  color: var(--text-muted);
  margin-bottom: var(--spacing-xl);
  padding-bottom: var(--spacing-lg);
  border-bottom: 1px solid var(--border-light);
  display: flex;
  justify-content: center;
  align-items: center;
  gap: var(--spacing-md);
}

.post-meta .author,
.post-meta .date {
  color: var(--text-muted);
  font-size: var(--font-size-sm);
}

.post-content {
  line-height: var(--line-height-reading);
  color: var(--text-primary);
  font-size: var(--font-size-base);
  white-space: pre-wrap;
}
</style>
