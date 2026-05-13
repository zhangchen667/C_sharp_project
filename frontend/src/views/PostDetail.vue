<template>
  <div class="post-detail">
    <el-button class="back-btn" @click="$router.back()">
      <el-icon><ArrowLeft /></el-icon>
      返回列表
    </el-button>
    <el-card class="detail-card">
      <div class="detail-header">
        <h1 class="post-title">{{ post.title }}</h1>
        <el-button v-if="userStore.isLoggedIn && post.authorId === userStore.user?.id" type="danger" size="small" @click="deletePost">删除文章</el-button>
      </div>
      <div class="post-meta">
        <el-tag size="small">{{ post.categoryName }}</el-tag>
        <span class="author">{{ post.authorName || '匿名' }}</span>
        <span class="date">{{ formatDate(post.createdAt) }}</span>
      </div>
      <div class="post-content markdown-body" v-html="renderedContent"></div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { ArrowLeft } from '@element-plus/icons-vue'
import { postsApi } from '../api'
import { marked } from 'marked'
import { useUserStore } from '../stores/user'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()
const post = ref({ title: '', content: '' })

const renderedContent = computed(() => {
  return marked(post.value.content || '')
})

const fetchPost = async () => {
  const res = await postsApi.getDetail(route.params.id)
  post.value = res
}

const formatDate = (date) => {
  return new Date(date).toLocaleString()
}

const deletePost = async () => {
  try {
    await ElMessageBox.confirm(`确定删除文章「${post.value.title}」吗？`, '提示', { type: 'warning' })
    const res = await postsApi.delete(route.params.id)
    if (res.success) {
      ElMessage.success('删除成功')
      router.push('/posts')
    }
  } catch {
    // 用户取消
  }
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

.detail-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
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
}

.markdown-body h1, .markdown-body h2, .markdown-body h3,
.markdown-body h4, .markdown-body h5, .markdown-body h6 {
  margin-top: 1.5em;
  margin-bottom: 0.5em;
  font-family: var(--font-family-serif);
}

.markdown-body p {
  margin-bottom: 1em;
}

.markdown-body ul, .markdown-body ol {
  padding-left: 2em;
  margin-bottom: 1em;
}

.markdown-body code {
  background: var(--bg-secondary);
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 0.9em;
}

.markdown-body pre {
  background: var(--bg-secondary);
  padding: 1em;
  border-radius: 8px;
  overflow-x: auto;
  margin-bottom: 1em;
}

.markdown-body pre code {
  background: none;
  padding: 0;
}

.markdown-body blockquote {
  border-left: 4px solid var(--brand-primary);
  padding-left: 1em;
  margin: 1em 0;
  color: var(--text-secondary);
}

.markdown-body img {
  max-width: 100%;
  border-radius: 8px;
}

.markdown-body a {
  color: var(--brand-primary);
}

.markdown-body table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 1em;
}

.markdown-body th, .markdown-body td {
  border: 1px solid var(--border-light);
  padding: 8px 12px;
  text-align: left;
}
</style>
