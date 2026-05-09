<template>
  <div class="albums">
    <div class="header-bar">
      <h2>相册</h2>
      <el-button type="primary" @click="showUpload = true" v-if="userStore.isAdmin">上传照片</el-button>
    </div>

    <el-empty v-if="!photos.length" description="暂无照片" />

    <el-row :gutter="20" v-else>
      <el-col :span="6" v-for="photo in photos" :key="photo.id">
        <el-card shadow="hover" class="photo-card">
          <el-image
            :src="`http://localhost:5000${photo.filePath}`"
            fit="cover"
            style="width: 100%; height: 200px"
            :preview-src-list="[`http://localhost:5000${photo.filePath}`]"
          />
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
      <el-input v-model="uploadDesc" placeholder="描述（选填）" class="mt-2" />
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
  padding: 20px;
}

.header-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.header-bar h2 {
  margin: 0;
}

.photo-card {
  margin-bottom: 20px;
}

.photo-info {
  margin-top: 10px;
}

.photo-desc {
  margin: 0 0 8px;
  font-size: 14px;
  color: #333;
}

.photo-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.date {
  font-size: 12px;
  color: #999;
}

.mt-2 {
  margin-top: 10px;
}
</style>
