<template>
  <div class="posts">
    <el-card class="filter-card">
      <el-input v-model="keyword" placeholder="搜索文章" style="width: 300px" class="mr-2" />
      <el-select v-model="categoryId" placeholder="选择分类" clearable style="width: 150px">
        <el-option v-for="cat in categories" :key="cat.id" :label="cat.name" :value="cat.id" />
      </el-select>
      <el-button type="primary" @click="fetchPosts">搜索</el-button>
      <el-button type="success" @click="showCreate = true" v-if="userStore.isAdmin">发布文章</el-button>
    </el-card>

    <el-row :gutter="20">
      <el-col :span="12" v-for="post in posts" :key="post.id">
        <el-card shadow="hover" class="post-card">
          <h3 class="post-title" @click="$router.push(`/posts/${post.id}`)">{{ post.title }}</h3>
          <div class="post-meta">
            <el-tag size="small">{{ post.categoryName }}</el-tag>
            <span class="author">{{ post.authorName || '匿名' }}</span>
            <span class="date">{{ formatDate(post.createdAt) }}</span>
          </div>
          <div class="post-excerpt">{{ post.content.substring(0, 100) }}...</div>
          <el-button type="primary" link @click="$router.push(`/posts/${post.id}`)">阅读全文</el-button>
        </el-card>
      </el-col>
    </el-row>

    <el-dialog v-model="showCreate" title="发布文章" width="600px">
      <el-form :model="postForm" label-width="80px">
        <el-form-item label="标题">
          <el-input v-model="postForm.title" />
        </el-form-item>
        <el-form-item label="分类">
          <el-select v-model="postForm.categoryId" style="width: 100%">
            <el-option v-for="cat in categories" :key="cat.id" :label="cat.name" :value="cat.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="内容">
          <el-input v-model="postForm.content" type="textarea" :rows="10" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showCreate = false">取消</el-button>
        <el-button type="primary" @click="createPost">发布</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { postsApi } from '../api'
import { useUserStore } from '../stores/user'

const userStore = useUserStore()
const keyword = ref('')
const categoryId = ref()
const categories = ref([])
const posts = ref([])
const showCreate = ref(false)
const postForm = ref({ title: '', content: '', categoryId: 1, isPublic: true })

const fetchCategories = async () => {
  const res = await postsApi.getCategories()
  categories.value = res
}

const fetchPosts = async () => {
  const res = await postsApi.getList({ keyword: keyword.value, categoryId: categoryId.value })
  posts.value = res
}

const createPost = async () => {
  if (!postForm.value.title || !postForm.value.content) {
    ElMessage.error('请填写完整信息')
    return
  }
  try {
    const res = await postsApi.create(postForm.value)
    if (res.success) {
      ElMessage.success('发布成功')
      showCreate.value = false
      fetchPosts()
    }
  } catch (e) {
    ElMessage.error('发布失败')
  }
}

const formatDate = (date) => {
  return new Date(date).toLocaleDateString()
}

onMounted(() => {
  fetchCategories()
  fetchPosts()
})
</script>

<style scoped>
.posts {
  padding: 20px;
}

.filter-card {
  margin-bottom: 20px;
}

.mr-2 {
  margin-right: 10px;
}

.post-card {
  margin-bottom: 20px;
}

.post-title {
  font-size: 18px;
  margin: 0 0 10px;
  cursor: pointer;
  color: #303133;
}

.post-title:hover {
  color: #409eff;
}

.post-meta {
  margin-bottom: 10px;
  color: #999;
  font-size: 12px;
}

.post-meta .author,
.post-meta .date {
  margin-left: 10px;
}

.post-excerpt {
  color: #666;
  line-height: 1.6;
}
</style>
