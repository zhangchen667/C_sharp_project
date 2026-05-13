<template>
  <div class="posts">
    <h1 class="page-title">博客文章</h1>

    <el-card class="filter-card">
      <div class="filter-row">
        <el-input v-model="keyword" placeholder="搜索文章" class="filter-input" />
        <el-select v-model="categoryId" placeholder="选择分类" clearable class="filter-select">
          <el-option v-for="cat in categories" :key="cat.id" :label="cat.name" :value="cat.id" />
        </el-select>
        <el-button type="primary" @click="fetchPosts">搜索</el-button>
        <el-button type="primary" @click="showCreate = true" v-if="userStore.isLoggedIn">发布文章</el-button>
      </div>
    </el-card>

    <el-row :gutter="24">
      <el-col :lg="12" :md="12" :sm="24" v-for="post in posts" :key="post.id">
        <el-card shadow="hover" class="post-card">
          <h3 class="post-title" @click="$router.push(`/posts/${post.id}`)">{{ post.title }}</h3>
          <div class="post-meta">
            <el-tag size="small">{{ post.categoryName }}</el-tag>
            <span class="author">{{ post.authorName || '匿名' }}</span>
            <span class="date">{{ formatDate(post.createdAt) }}</span>
          </div>
          <div class="post-excerpt">{{ post.content.substring(0, 100) }}...</div>
          <el-button type="primary" link @click="$router.push(`/posts/${post.id}`)">阅读全文 →</el-button>
          <el-button v-if="userStore.isLoggedIn" type="danger" link @click.stop="deletePost(post.id, post.title)">删除</el-button>
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
import { ElMessage, ElMessageBox } from 'element-plus'
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

const deletePost = async (id, title) => {
  try {
    await ElMessageBox.confirm(`确定删除文章「${title}」吗？`, '提示', { type: 'warning' })
    const res = await postsApi.delete(id)
    if (res.success) {
      ElMessage.success('删除成功')
      fetchPosts()
    }
  } catch {
    // 用户取消
  }
}

onMounted(() => {
  fetchCategories()
  fetchPosts()
})
</script>

<style scoped>
.posts {
  position: relative;
  z-index: 1;
}

.page-title {
  font-family: var(--font-family-serif);
  font-size: var(--font-size-2xl);
  margin-bottom: var(--spacing-lg);
  color: var(--text-primary);
}

.filter-card {
  margin-bottom: var(--spacing-xl);
}

.filter-row {
  display: flex;
  gap: var(--spacing-md);
  align-items: center;
  flex-wrap: wrap;
}

.filter-input {
  flex: 0 0 300px;
}

.filter-select {
  flex: 0 0 150px;
}

.post-card {
  margin-bottom: var(--spacing-lg);
  height: calc(100% - var(--spacing-lg));
  display: flex;
  flex-direction: column;
}

.post-title {
  font-family: var(--font-family-serif);
  font-size: var(--font-size-lg);
  margin: 0 0 var(--spacing-md);
  cursor: pointer;
  color: var(--text-primary);
  transition: color var(--transition-fast);
  line-height: 1.4;
}

.post-title:hover {
  color: var(--brand-primary);
}

.post-meta {
  margin-bottom: var(--spacing-md);
  font-size: var(--font-size-xs);
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  flex-wrap: wrap;
}

.post-meta .author,
.post-meta .date {
  color: var(--text-muted);
}

.post-excerpt {
  color: var(--text-secondary);
  line-height: var(--line-height-reading);
  font-size: var(--font-size-sm);
  margin-bottom: var(--spacing-md);
  flex: 1;
}

@media (max-width: 768px) {
  .filter-row {
    flex-direction: column;
    align-items: stretch;
  }

  .filter-input,
  .filter-select {
    flex: none;
  }
}
</style>
