<template>
  <div class="comments">
    <h2 class="page-title">留言板</h2>

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
      <h3>全部留言</h3>
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
  padding: 20px;
}

.page-title {
  margin: 0 0 20px;
}

.comment-form {
  margin-bottom: 30px;
}

.comment-list h3 {
  margin: 0 0 20px;
}

.comment-item {
  margin-bottom: 15px;
}

.comment-header {
  margin-bottom: 10px;
}

.comment-header .author {
  font-weight: bold;
  color: #409eff;
}

.comment-header .date {
  margin-left: 15px;
  color: #999;
  font-size: 12px;
}

.comment-content {
  color: #333;
  line-height: 1.6;
}

.comment-reply {
  margin-top: 15px;
  padding: 10px 15px;
  background: #f0f9eb;
  border-left: 4px solid #67c23a;
  color: #67c23a;
  border-radius: 4px;
}
</style>
