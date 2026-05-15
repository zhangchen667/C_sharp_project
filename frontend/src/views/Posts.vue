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
            <span class="image-count" v-if="post.imageCount > 0">
              <el-icon><Picture /></el-icon> {{ post.imageCount }}张
            </span>
            <span class="comment-count">
              <el-icon><ChatDotRound /></el-icon> {{ post.commentCount || 0 }}
            </span>
          </div>
          <div class="post-excerpt markdown-body" v-html="getExcerpt(post.content)"></div>
          <div class="post-actions">
            <el-button class="link-btn" @click="$router.push(`/posts/${post.id}`)">阅读全文 →</el-button>
            <el-button
              v-if="userStore.isLoggedIn && post.authorId === userStore.user?.id"
              class="delete-btn"
              @click.stop="deletePost(post.id, post.title)"
            >删除</el-button>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-dialog v-model="showCreate" title="发布文章" width="650px">
      <el-form :model="postForm" label-width="80px">
        <el-form-item label="标题">
          <el-input v-model="postForm.title" placeholder="请输入文章标题" />
        </el-form-item>
        <el-form-item label="分类">
          <el-select v-model="postForm.categoryId" style="width: 100%">
            <el-option v-for="cat in categories" :key="cat.id" :label="cat.name" :value="cat.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="内容">
          <el-input v-model="postForm.content" type="textarea" :rows="8" placeholder="请输入文章内容" />
        </el-form-item>
        <el-form-item label="图片">
          <div class="image-upload-area">
            <el-upload
              action=""
              :auto-upload="false"
              :on-change="handleImageUpload"
              :show-file-list="false"
              accept="image/*"
              multiple
            >
              <div class="upload-btn">
                <el-icon :size="24"><Plus /></el-icon>
                <p>添加图片</p>
              </div>
            </el-upload>
            <div class="image-preview-list">
              <div v-for="(img, index) in uploadedImages" :key="index" class="image-preview-item">
                <img :src="img.url" :alt="img.name" class="preview-img" />
                <el-button
                  type="danger"
                  size="small"
                  circle
                  @click.stop="removeImage(index)"
                  class="remove-btn"
                >
                  <el-icon><Close /></el-icon>
                </el-button>
              </div>
            </div>
          </div>
          <p class="upload-tip">支持拖拽上传，最多可上传 9 张图片</p>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showCreate = false">取消</el-button>
        <el-button type="primary" @click="createPost" :loading="submitting">发布</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Picture, ChatDotRound, Plus, Close } from '@element-plus/icons-vue'
import { postsApi } from '../api'
import { useUserStore } from '../stores/user'
import { marked } from 'marked'

const userStore = useUserStore()
const keyword = ref('')
const categoryId = ref()
const categories = ref([])
const posts = ref([])
const showCreate = ref(false)
const submitting = ref(false)
const postForm = ref({ title: '', content: '', categoryId: 1, isPublic: true })
const uploadedImages = ref([])

const fetchCategories = async () => {
  const res = await postsApi.getCategories()
  categories.value = res
}

const fetchPosts = async () => {
  const res = await postsApi.getList({ keyword: keyword.value, categoryId: categoryId.value })
  posts.value = res.posts || res
  await nextTick()
  if (window.MathJax && window.MathJax.typesetPromise) {
    window.MathJax.typesetPromise()
  }
}

const handleImageUpload = async (file) => {
  if (uploadedImages.value.length >= 9) {
    ElMessage.warning('最多只能上传 9 张图片')
    return
  }

  const formData = new FormData()
  formData.append('file', file.raw)

  try {
    const res = await postsApi.uploadImage(formData)
    if (res.success) {
      uploadedImages.value.push({
        url: res.filePath,
        name: res.fileName || file.name
      })
      ElMessage.success('图片上传成功')
    }
  } catch (e) {
    ElMessage.error('图片上传失败')
  }
}

const removeImage = (index) => {
  uploadedImages.value.splice(index, 1)
}

const createPost = async () => {
  if (!postForm.value.title || !postForm.value.content) {
    ElMessage.error('请填写完整信息')
    return
  }

  submitting.value = true
  try {
    const data = {
      ...postForm.value,
      imagePaths: uploadedImages.value.map(img => img.url)
    }

    const res = await postsApi.create(data)
    if (res.success) {
      ElMessage.success('发布成功')
      showCreate.value = false
      resetForm()
      fetchPosts()
    }
  } catch (e) {
    ElMessage.error('发布失败')
  } finally {
    submitting.value = false
  }
}

const resetForm = () => {
  postForm.value = { title: '', content: '', categoryId: 1, isPublic: true }
  uploadedImages.value = []
}

const formatDate = (date) => {
  if (!date) return ''
  return new Date(date).toLocaleDateString()
}

const getExcerpt = (content) => {
  const text = (content || '').substring(0, 100)
  return marked(text) + '...'
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

.image-count,
.comment-count {
  color: var(--text-muted);
  display: flex;
  align-items: center;
  gap: 4px;
}

.post-excerpt {
  color: var(--text-secondary);
  line-height: var(--line-height-reading);
  font-size: var(--font-size-sm);
  margin-bottom: var(--spacing-md);
  flex: 1;
}

.post-actions {
  display: flex;
  gap: var(--spacing-md);
  align-items: center;
}

.link-btn {
  color: var(--brand-primary);
  padding: 0;
  font-size: var(--font-size-sm);
  transition: color var(--transition-fast);
  border: none;
  background: transparent;
  cursor: pointer;
}

.link-btn:hover {
  color: var(--brand-primary-deep);
}

.delete-btn {
  color: var(--text-muted);
  padding: 0;
  font-size: var(--font-size-sm);
  transition: color var(--transition-fast);
  border: none;
  background: transparent;
  cursor: pointer;
}

.delete-btn:hover {
  color: #D45D5D;
}

.image-upload-area {
  display: flex;
  gap: var(--spacing-md);
  flex-wrap: wrap;
  align-items: flex-start;
}

.upload-btn {
  width: 100px;
  height: 100px;
  border: 2px dashed var(--border-light);
  border-radius: var(--radius-md);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
  cursor: pointer;
  transition: all 0.2s;
}

.upload-btn:hover {
  border-color: var(--brand-primary);
  color: var(--brand-primary);
}

.upload-btn p {
  margin: 4px 0 0;
  font-size: var(--font-size-xs);
}

.image-preview-list {
  display: flex;
  gap: var(--spacing-md);
  flex-wrap: wrap;
}

.image-preview-item {
  position: relative;
  width: 100px;
  height: 100px;
  border-radius: var(--radius-md);
  overflow: hidden;
}

.preview-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.remove-btn {
  position: absolute;
  top: 4px;
  right: 4px;
  padding: 4px;
}

.upload-tip {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  margin-top: var(--spacing-sm);
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
