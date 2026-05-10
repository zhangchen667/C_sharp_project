<template>
  <div class="albums">
    <div class="header-bar">
      <h1 class="page-title">相册</h1>
      <el-button type="primary" @click="showUpload = true" v-if="userStore.isAdmin">上传照片</el-button>
    </div>

    <el-empty v-if="!photos.length" description="暂无照片" />

    <el-row :gutter="24" v-else>
      <el-col :lg="6" :md="8" :sm="12" :xs="24" v-for="photo in photos" :key="photo.id">
        <el-card shadow="hover" class="photo-card">
          <div class="image-wrapper">
            <el-image
              :src="`http://localhost:5000${photo.filePath}`"
              fit="cover"
              style="width: 100%; height: 200px"
              :preview-src-list="[`http://localhost:5000${photo.filePath}`]"
              :preview-teleported="true"
            />
          </div>
          <div class="photo-info">
            <p class="photo-desc">{{ photo.description || photo.fileName }}</p>
            <div class="photo-actions">
              <span class="date">{{ formatDate(photo.uploadedAt) }}</span>
              <el-button type="danger" link size="small" v-if="userStore.isAdmin" @click="deletePhoto(photo.id)">删除</el-button>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-dialog v-model="showUpload" title="上传照片" width="500px">
      <el-upload
        ref="uploadRef"
        action=""
        :auto-upload="false"
        :limit="1"
        accept="image/*"
        :on-change="handleFileChange"
      >
        <el-button type="primary">选择图片</el-button>
      </el-upload>
      <el-input v-model="uploadDesc" placeholder="描述（选填）" class="desc-input" />
      <template #footer>
        <el-button @click="showUpload = false">取消</el-button>
        <el-button type="primary" @click="uploadPhoto" :loading="uploading">上传</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { albumsApi } from '../api'
import { useUserStore } from '../stores/user'

const userStore = useUserStore()
const photos = ref([])
const showUpload = ref(false)
const uploading = ref(false)
const uploadFile = ref(null)
const uploadDesc = ref('')

const fetchPhotos = async () => {
  const res = await albumsApi.getPhotos()
  photos.value = res
}

const handleFileChange = (file) => {
  uploadFile.value = file.raw
}

const uploadPhoto = async () => {
  if (!uploadFile.value) {
    ElMessage.error('请选择图片')
    return
  }
  uploading.value = true
  const formData = new FormData()
  formData.append('file', uploadFile.value)
  formData.append('description', uploadDesc.value)
  try {
    const res = await albumsApi.upload(formData)
    if (res.success) {
      ElMessage.success('上传成功')
      showUpload.value = false
      fetchPhotos()
    }
  } catch (e) {
    ElMessage.error('上传失败')
  } finally {
    uploading.value = false
  }
}

const deletePhoto = async (id) => {
  try {
    await albumsApi.delete(id)
    ElMessage.success('删除成功')
    fetchPhotos()
  } catch (e) {
    ElMessage.error('删除失败')
  }
}

const formatDate = (date) => {
  return new Date(date).toLocaleDateString()
}

onMounted(fetchPhotos)
</script>

<style scoped>
.albums {
  position: relative;
  z-index: 1;
}

.header-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-xl);
}

.page-title {
  font-family: var(--font-family-serif);
  font-size: var(--font-size-2xl);
  margin: 0;
  color: var(--text-primary);
}

.photo-card {
  margin-bottom: var(--spacing-lg);
}

.image-wrapper {
  margin: calc(var(--spacing-lg) * -1) calc(var(--spacing-lg) * -1) var(--spacing-md);
  border-radius: var(--radius-lg) var(--radius-lg) 0 0;
  overflow: hidden;
}

.photo-info {
  margin-top: var(--spacing-sm);
}

.photo-desc {
  margin: 0 0 var(--spacing-sm);
  font-size: var(--font-size-sm);
  color: var(--text-primary);
  line-height: 1.4;
}

.photo-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.date {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}

.desc-input {
  margin-top: var(--spacing-md);
}
</style>
