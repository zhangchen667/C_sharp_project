<template>
  <div class="post-detail">
    <el-page-header @back="$router.back()" content="返回列表" />
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
  padding: 20px;
}

.detail-card {
  margin-top: 20px;
}

.post-title {
  font-size: 28px;
  margin: 0 0 20px;
  text-align: center;
}

.post-meta {
  text-align: center;
  color: #999;
  margin-bottom: 30px;
  padding-bottom: 20px;
  border-bottom: 1px solid #eee;
}

.post-meta .author,
.post-meta .date {
  margin-left: 15px;
}

.post-content {
  line-height: 2;
  color: #333;
  font-size: 16px;
  white-space: pre-wrap;
}
</style>
