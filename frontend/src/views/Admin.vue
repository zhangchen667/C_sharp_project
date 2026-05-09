<template>
  <div class="admin">
    <h2 class="page-title">管理后台</h2>

    <el-row :gutter="20" class="stat-row">
      <el-col :span="6" v-for="stat in stats" :key="stat.title">
        <el-card shadow="hover">
          <el-statistic :title="stat.title" :value="stat.value" />
        </el-card>
      </el-col>
    </el-row>

    <el-card class="comments-card">
      <template #header>
        <span>留言管理</span>
      </template>
      <el-table :data="comments" style="width: 100%">
        <el-table-column prop="authorName" label="昵称" width="120" />
        <el-table-column prop="email" label="邮箱" width="180" />
        <el-table-column prop="content" label="留言内容" />
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button size="small" @click="showReply(row)">回复</el-button>
            <el-button size="small" type="danger" @click="deleteComment(row.id)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="showReplyDialog" title="回复留言" width="500px">
      <el-input v-model="replyContent" type="textarea" :rows="4" placeholder="请输入回复内容" />
      <template #footer>
        <el-button @click="showReplyDialog = false">取消</el-button>
        <el-button type="primary" @click="submitReply">提交回复</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { commentsApi, postsApi, albumsApi } from '../api'

const comments = ref([])
const stats = ref([
  { title: '文章数量', value: 0 },
  { title: '照片数量', value: 0 },
  { title: '留言总数', value: 0 },
  { title: '待回复', value: 0 }
])
const showReplyDialog = ref(false)
const replyContent = ref('')
let currentCommentId = null

const fetchData = async () => {
  const [commentsRes, postsRes, photosRes] = await Promise.all([
    commentsApi.getAll(),
    postsApi.getList(),
    albumsApi.getPhotos()
  ])

  comments.value = commentsRes
  stats.value[0].value = postsRes.length
  stats.value[1].value = photosRes.length
  stats.value[2].value = commentsRes.length
  stats.value[3].value = commentsRes.filter(c => !c.reply).length
}

const showReply = (row) => {
  currentCommentId = row.id
  replyContent.value = row.reply || ''
  showReplyDialog.value = true
}

const submitReply = async () => {
  if (!replyContent.value) {
    ElMessage.error('请输入回复内容')
    return
  }
  try {
    await commentsApi.reply(currentCommentId, replyContent.value)
    ElMessage.success('回复成功')
    showReplyDialog.value = false
    fetchData()
  } catch (e) {
    ElMessage.error('回复失败')
  }
}

const deleteComment = async (id) => {
  try {
    await commentsApi.delete(id)
    ElMessage.success('删除成功')
    fetchData()
  } catch (e) {
    ElMessage.error('删除失败')
  }
}

onMounted(fetchData)
</script>

<style scoped>
.admin {
  padding: 20px;
}

.page-title {
  margin: 0 0 20px;
}

.stat-row {
  margin-bottom: 30px;
}

.comments-card {
  margin-top: 20px;
}
</style>
