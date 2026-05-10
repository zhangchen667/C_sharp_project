<template>
  <div class="comments">
    <h1 class="page-title">留言板</h1>

    <el-card class="comment-form">
      <el-form :model="form" label-width="80px">
        <el-form-item label="昵称">
          <el-input v-model="form.authorName" placeholder="请输入昵称" />
        </el-form-item>
        <el-form-item label="邮箱">
          <el-input v-model="form.email" placeholder="请输入邮箱（选填）" />
        </el-form-item>
        <el-form-item label="留言">
          <el-input v-model="form.content" type="textarea" :rows="4" placeholder="请输入留言内容" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="submitComment">提交留言</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <div class="comment-list">
      <h2>全部留言</h2>
      <el-empty v-if="!comments.length" description="暂无留言" />
      <el-card v-for="comment in comments" :key="comment.id" shadow="hover" class="comment-item">
        <div class="comment-header">
          <span class="author">{{ comment.authorName }}</span>
          <span class="date">{{ formatDate(comment.createdAt) }}</span>
        </div>
        <div class="comment-content">{{ comment.content }}</div>
        <div v-if="comment.reply" class="comment-reply">
          <strong>站长回复：</strong> {{ comment.reply }}
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { commentsApi } from '../api'

const form = ref({ authorName: '', email: '', content: '' })
const comments = ref([])

const fetchComments = async () => {
  const res = await commentsApi.getList()
  comments.value = res
}

const submitComment = async () => {
  if (!form.value.authorName || !form.value.content) {
    ElMessage.error('请填写昵称和留言内容')
    return
  }
  try {
    const res = await commentsApi.create(form.value)
    if (res.success) {
      ElMessage.success('留言成功')
      form.value = { authorName: '', email: '', content: '' }
      fetchComments()
    }
  } catch (e) {
    ElMessage.error('留言失败')
  }
}

const formatDate = (date) => {
  return new Date(date).toLocaleString()
}

onMounted(fetchComments)
</script>

<style scoped>
.comments {
  position: relative;
  z-index: 1;
}

.page-title {
  font-family: var(--font-family-serif);
  font-size: var(--font-size-2xl);
  margin: 0 0 var(--spacing-xl);
  color: var(--text-primary);
}

.comment-form {
  margin-bottom: var(--spacing-2xl);
}

.comment-list h2 {
  font-family: var(--font-family-serif);
  font-size: var(--font-size-xl);
  margin: 0 0 var(--spacing-lg);
  color: var(--text-primary);
}

.comment-item {
  margin-bottom: var(--spacing-md);
}

.comment-header {
  margin-bottom: var(--spacing-md);
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.comment-header .author {
  font-weight: 600;
  color: var(--brand-primary);
}

.comment-header .date {
  color: var(--text-muted);
  font-size: var(--font-size-xs);
}

.comment-content {
  color: var(--text-primary);
  line-height: var(--line-height-reading);
}

.comment-reply {
  margin-top: var(--spacing-md);
  padding: var(--spacing-md) var(--spacing-lg);
  background: var(--bg-secondary);
  border-left: 4px solid var(--brand-secondary);
  color: var(--text-secondary);
  border-radius: 0 var(--radius-md) var(--radius-md) 0;
  font-size: var(--font-size-sm);
}
</style>
