<template>
  <div class="profile-page">
    <!-- 头部用户信息 -->
    <div class="profile-header">
      <div class="avatar-wrapper">
        <img v-if="profile.avatar" :src="profile.avatar" :alt="profile.nickname" class="avatar" />
        <div v-else class="avatar-placeholder">
          <el-icon :size="48"><User /></el-icon>
        </div>
      </div>
      <div class="profile-info">
        <h1 class="nickname">{{ profile.nickname || profile.userName }}</h1>
        <p class="bio" v-if="profile.bio">{{ profile.bio }}</p>
        <div class="stats">
          <span class="stat-item">
            <strong>{{ profile.postCount || 0 }}</strong> 篇文章
          </span>
          <span class="stat-item">
            <strong>{{ profile.photoCount || 0 }}</strong> 张照片
          </span>
        </div>
        <div class="join-date">
          <el-icon><Calendar /></el-icon>
          {{ formatDate(profile.createdAt) }} 加入
        </div>
      </div>
    </div>

    <!-- 相册区域 -->
    <div class="section" v-if="isOwner || photos.length">
      <div class="section-header">
        <h2>
          <el-icon><Picture /></el-icon>
          相册
        </h2>
        <el-button
          v-if="isOwner"
          type="primary"
          size="small"
          @click="showUpload = true"
        >
          <el-icon><Upload /></el-icon> 上传照片
        </el-button>
      </div>

      <el-empty v-if="!photos.length" description="暂无照片" />

      <div class="masonry-grid" v-else>
        <div
          v-for="photo in photos"
          :key="photo.id"
          class="photo-item"
        >
          <el-image
            :src="getImageUrl(photo.filePath)"
            fit="cover"
            :preview-src-list="[getImageUrl(photo.filePath)]"
            class="photo-img"
          />
          <div class="photo-actions" v-if="isOwner">
            <el-button
              type="danger"
              size="small"
              link
              @click.stop="deletePhoto(photo.id)"
            >
              删除
            </el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- 文章列表 -->
    <div class="section">
      <div class="section-header">
        <h2>
          <el-icon><Document /></el-icon>
          发布的文章
        </h2>
      </div>

      <el-empty v-if="!posts.length" description="暂无文章" />

      <div class="posts-list" v-else>
        <div
          v-for="post in posts"
          :key="post.id"
          class="post-card"
          @click="$router.push(`/posts/${post.id}`)"
        >
          <h3 class="post-title">{{ post.title }}</h3>
          <div class="post-excerpt">
            {{ (post.content || '').substring(0, 150) }}...
          </div>
          <div class="post-meta">
            <el-tag size="small">{{ post.categoryName }}</el-tag>
            <span class="date">{{ formatDate(post.createdAt) }}</span>
            <span class="comment-count">
              <el-icon><ChatDotRound /></el-icon>
              {{ post.commentCount || 0 }}
            </span>
          </div>
        </div>
      </div>

      <el-pagination
        v-if="totalPosts > pageSize"
        class="pagination"
        :total="totalPosts"
        :page-size="pageSize"
        v-model:current-page="currentPage"
        layout="prev, pager, next"
        @current-change="loadPosts"
      />
    </div>

    <!-- 照片上传对话框 -->
    <el-dialog v-model="showUpload" title="上传照片" width="500px">
      <el-upload
        ref="uploadRef"
        action=""
        :auto-upload="false"
        :limit="1"
        accept="image/*"
        :on-change="handleFileChange"
        :show-file-list="false"
      >
        <div class="upload-placeholder">
          <el-icon :size="48"><Upload /></el-icon>
          <p>点击选择图片</p>
        </div>
      </el-upload>

      <el-input
        v-model="photoDesc"
        placeholder="照片描述（可选）"
        type="textarea"
        :rows="2"
        class="mt-4"
      />

      <template #footer>
        <el-button @click="showUpload = false">取消</el-button>
        <el-button type="primary" @click="uploadPhoto" :loading="uploading">
          上传
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { User, Calendar, Picture, Document, Upload, ChatDotRound } from '@element-plus/icons-vue'
import { profileApi } from '../api'
import { useUserStore } from '../stores/user'

const route = useRoute()
const userStore = useUserStore()

const profile = ref({})
const photos = ref([])
const posts = ref([])
const showUpload = ref(false)
const uploading = ref(false)
const uploadFile = ref(null)
const photoDesc = ref('')
const currentPage = ref(1)
const pageSize = ref(10)
const totalPosts = ref(0)

const userId = computed(() => route.params.userId || userStore.user?.id)
const isOwner = computed(() => userStore.isLoggedIn && userId.value === userStore.user?.id)

const getImageUrl = (path) => {
  if (!path) return ''
  return path.startsWith('http') ? path : 'http://localhost:5000' + path
}

const formatDate = (date) => {
  if (!date) return ''
  return new Date(date).toLocaleDateString('zh-CN')
}

const loadProfile = async () => {
  try {
    profile.value = await profileApi.getProfile(userId.value)
  } catch (e) {
    ElMessage.error('加载用户信息失败')
  }
}

const loadPhotos = async () => {
  try {
    photos.value = await profileApi.getPhotos(userId.value)
  } catch (e) {
    console.error('加载照片失败', e)
  }
}

const loadPosts = async () => {
  try {
    const res = await profileApi.getPosts(userId.value, {
      page: currentPage.value,
      pageSize: pageSize.value
    })
    posts.value = res.posts || []
    totalPosts.value = res.totalCount || 0
  } catch (e) {
    console.error('加载文章失败', e)
  }
}

const handleFileChange = (file) => {
  uploadFile.value = file.raw
}

const uploadPhoto = async () => {
  if (!uploadFile.value) {
    ElMessage.warning('请选择图片')
    return
  }

  uploading.value = true
  try {
    const formData = new FormData()
    formData.append('file', uploadFile.value)
    if (photoDesc.value) {
      formData.append('description', photoDesc.value)
    }

    const res = await profileApi.uploadPhoto(formData)
    if (res.success) {
      ElMessage.success('上传成功')
      showUpload.value = false
      uploadFile.value = null
      photoDesc.value = ''
      loadPhotos()
    }
  } catch (e) {
    ElMessage.error('上传失败')
  } finally {
    uploading.value = false
  }
}

const deletePhoto = async (id) => {
  try {
    await ElMessageBox.confirm('确定删除这张照片吗？', '提示', { type: 'warning' })
    const res = await profileApi.deletePhoto(id)
    if (res.success) {
      ElMessage.success('删除成功')
      loadPhotos()
    }
  } catch { /* 用户取消 */ }
}

onMounted(() => {
  loadProfile()
  loadPhotos()
  loadPosts()
})
</script>

<style scoped>
.profile-page {
  position: relative;
  z-index: 1;
}

.profile-header {
  background: var(--bg-card);
  border-radius: var(--radius-lg);
  padding: var(--spacing-xl);
  display: flex;
  gap: var(--spacing-xl);
  margin-bottom: var(--spacing-xl);
  align-items: center;
}

.avatar-wrapper {
  flex-shrink: 0;
}

.avatar {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  object-fit: cover;
  border: 4px solid var(--border-light);
}

.avatar-placeholder {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  background: var(--bg-secondary);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
  border: 4px solid var(--border-light);
}

.profile-info {
  flex: 1;
}

.nickname {
  font-family: var(--font-serif);
  font-size: var(--font-size-2xl);
  margin: 0 0 var(--spacing-sm);
  color: var(--text-primary);
}

.bio {
  color: var(--text-secondary);
  margin-bottom: var(--spacing-md);
  line-height: 1.6;
}

.stats {
  display: flex;
  gap: var(--spacing-xl);
  margin-bottom: var(--spacing-md);
}

.stat-item {
  color: var(--text-secondary);
}

.stat-item strong {
  color: var(--brand-primary);
  font-size: var(--font-size-lg);
  margin-right: 4px;
}

.join-date {
  color: var(--text-muted);
  font-size: var(--font-size-sm);
  display: flex;
  align-items: center;
  gap: 6px;
}

.section {
  background: var(--bg-card);
  border-radius: var(--radius-lg);
  padding: var(--spacing-xl);
  margin-bottom: var(--spacing-xl);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-lg);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--border-light);
}

.section-header h2 {
  font-family: var(--font-serif);
  font-size: var(--font-size-xl);
  margin: 0;
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--text-primary);
}

.masonry-grid {
  columns: 4;
  column-gap: var(--spacing-md);
}

.photo-item {
  break-inside: avoid;
  margin-bottom: var(--spacing-md);
  position: relative;
  border-radius: var(--radius-md);
  overflow: hidden;
}

.photo-img {
  width: 100%;
  display: block;
}

.photo-actions {
  position: absolute;
  top: 8px;
  right: 8px;
  opacity: 0;
  transition: opacity 0.2s;
}

.photo-item:hover .photo-actions {
  opacity: 1;
}

.upload-placeholder {
  text-align: center;
  padding: var(--spacing-xl);
  color: var(--text-muted);
  border: 2px dashed var(--border-light);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all 0.2s;
}

.upload-placeholder:hover {
  border-color: var(--brand-primary);
  color: var(--brand-primary);
}

.posts-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.post-card {
  padding: var(--spacing-lg);
  background: var(--bg-secondary);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all 0.2s;
}

.post-card:hover {
  background: rgba(212, 124, 107, 0.05);
  transform: translateX(4px);
}

.post-title {
  font-family: var(--font-serif);
  font-size: var(--font-size-lg);
  margin: 0 0 var(--spacing-sm);
  color: var(--text-primary);
}

.post-excerpt {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  line-height: 1.6;
  margin-bottom: var(--spacing-md);
}

.post-meta {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  font-size: var(--font-size-xs);
}

.post-meta .date {
  color: var(--text-muted);
}

.comment-count {
  color: var(--text-muted);
  display: flex;
  align-items: center;
  gap: 4px;
}

.pagination {
  display: flex;
  justify-content: center;
  margin-top: var(--spacing-xl);
}

@media (max-width: 768px) {
  .profile-header {
    flex-direction: column;
    text-align: center;
  }

  .masonry-grid {
    columns: 2;
  }
}
</style>
