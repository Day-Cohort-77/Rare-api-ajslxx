# Post Header Image Upload Feature

## Overview
This feature allows authors to upload and manage header images for their posts using Base64 encoding. Header images are stored directly in the database as data URLs, providing a seamless experience without external dependencies.

## 🎯 Key Features

✅ **Base64 Image Storage** - Images stored as data URLs in the database
✅ **Multiple Format Support** - JPEG, PNG, GIF, WebP
✅ **Image Validation** - File format and size validation
✅ **Size Limits** - 10MB maximum file size (larger than profile pictures)
✅ **Data URL Support** - Handles both raw base64 and data URL formats
✅ **CRUD Operations** - Upload, retrieve, and delete header images
✅ **Author Control** - Authors can update header images for their posts
✅ **Error Handling** - Comprehensive validation and error responses

## 📋 API Endpoints

### 1. Upload/Update Post Header Image
**POST** `/posts/{postId}/header-image`

**Request Body:**
```json
{
  "imageData": "data:image/jpeg;base64,/9j/4AAQSkZJRgABA...",
  "fileName": "blog-header.jpg",
  "contentType": "image/jpeg"
}
```

**Response (Success):**
```json
{
  "postId": 1,
  "imageUrl": "data:image/jpeg;base64,/9j/4AAQSkZJRgABA...",
  "fileName": "blog-header.jpg",
  "updatedOn": "2025-08-27T10:30:00Z",
  "success": true,
  "message": "Post header image updated successfully"
}
```

**Response (Error):**
```json
{
  "postId": 1,
  "imageUrl": "",
  "fileName": "",
  "updatedOn": "2025-08-27T10:30:00Z",
  "success": false,
  "message": "Header image size cannot exceed 10MB"
}
```

### 2. Get Post Header Image
**GET** `/posts/{postId}/header-image`

**Response:**
```json
{
  "postId": 1,
  "imageUrl": "data:image/jpeg;base64,/9j/4AAQSkZJRgABA..."
}
```

### 3. Delete Post Header Image
**DELETE** `/posts/{postId}/header-image`

**Response:** `204 No Content` (Success) or `404 Not Found`

## 🔧 Implementation Details

### Database Storage
- Images are stored in the existing `image_url` column as data URLs
- Format: `data:{mimeType};base64,{base64Data}`
- No database schema changes required

### Supported Formats
- **JPEG** (.jpg, .jpeg) - `image/jpeg`
- **PNG** (.png) - `image/png`
- **GIF** (.gif) - `image/gif`
- **WebP** (.webp) - `image/webp`

### Validation Rules
1. **Required Fields**: `imageData` and `fileName` are mandatory
2. **File Size**: Maximum 10MB (10,485,760 bytes) - larger than profile pictures
3. **Format Check**: Binary signature validation for image formats
4. **Base64 Validation**: Ensures valid base64 encoding

### Data URL Processing
The system accepts both formats:
- Raw Base64: `iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCA...`
- Data URL: `data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCA...`

## 💡 Frontend Integration Examples

### JavaScript File Upload
```javascript
// Convert file to base64
function fileToBase64(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => resolve(reader.result);
    reader.onerror = error => reject(error);
  });
}

// Upload post header image
async function uploadPostHeaderImage(postId, file) {
  try {
    const base64Data = await fileToBase64(file);
    
    const response = await fetch(`/posts/${postId}/header-image`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        imageData: base64Data,
        fileName: file.name,
        contentType: file.type
      })
    });
    
    const result = await response.json();
    
    if (result.success) {
      console.log('Header image updated!');
      // Update UI with new image
      document.getElementById('postHeaderImg').src = result.imageUrl;
    } else {
      alert('Error: ' + result.message);
    }
  } catch (error) {
    console.error('Upload failed:', error);
  }
}
```

### HTML Form Example
```html
<form id="headerImageForm">
  <input type="file" id="headerImage" accept="image/*" required>
  <button type="submit">Upload Header Image</button>
</form>

<script>
document.getElementById('headerImageForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  const file = document.getElementById('headerImage').files[0];
  if (file) {
    await uploadPostHeaderImage(1, file); // postId = 1
  }
});
</script>
```

### Display Header Image
```javascript
// Get and display header image
async function loadPostHeaderImage(postId) {
  try {
    const response = await fetch(`/posts/${postId}/header-image`);
    
    if (response.ok) {
      const data = await response.json();
      document.getElementById('postHeaderImg').src = data.imageUrl;
    } else {
      // Use default header or hide header
      document.getElementById('postHeaderImg').style.display = 'none';
    }
  } catch (error) {
    console.error('Failed to load header image:', error);
  }
}
```

### Blog Post Creation Flow
```javascript
// Create post and then upload header image
async function createPostWithHeader(postData, headerImageFile) {
  try {
    // 1. Create the post
    const postResponse = await fetch('/posts', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        ...postData,
        imageUrl: '' // Will be updated with header image
      })
    });
    
    const newPost = await postResponse.json();
    
    // 2. Upload header image to the new post
    if (headerImageFile) {
      await uploadPostHeaderImage(newPost.id, headerImageFile);
    }
    
    return newPost;
  } catch (error) {
    console.error('Failed to create post with header:', error);
  }
}
```

## 🧪 Testing

Use the provided test file: `post-header-image-tests.http`

### Test Scenarios Covered:
- ✅ Valid image upload (PNG, JPEG, WebP)
- ✅ Image retrieval
- ✅ Image deletion
- ✅ Validation errors (empty data, invalid base64)
- ✅ File size limits (10MB for headers vs 5MB for profiles)
- ✅ Multiple posts
- ✅ Post creation workflow

### Manual Testing Workflow:
1. **Create Post**: POST new post without header image
2. **Upload Header**: POST header image to the post
3. **Verify Storage**: GET post details to see updated image_url
4. **Retrieve Header**: GET header image endpoint
5. **Update Header**: POST with different image
6. **Delete Header**: DELETE endpoint
7. **Test Validation**: Try invalid data, oversized files

## 🔒 Security Considerations

### Current Implementation:
- File size limits (10MB maximum for headers)
- Image format validation (binary signatures)
- Base64 validation
- SQL injection protection (parameterized queries)

### Future Enhancements:
- Author verification (ensure user owns the post)
- Image dimension limits
- Content scanning for inappropriate images
- Rate limiting for uploads
- Image compression/optimization
- Backup/restore capabilities

## 🚀 Benefits of Base64 Storage

### Advantages:
✅ **No File System Management** - No need to handle file uploads, storage paths, or cleanup
✅ **No Broken Links** - Images are always available with the post data
✅ **Atomic Operations** - Image and post data updated together
✅ **Simple Deployment** - No additional storage configuration needed
✅ **Backup Included** - Images backed up with database

### Header Image Specific Benefits:
✅ **Larger Size Limit** - 10MB vs 5MB for profile pictures
✅ **Blog Enhancement** - Makes posts more visually appealing
✅ **Author Control** - Authors can update headers anytime
✅ **Consistent Display** - Headers always load with posts

## 📱 User Experience

### Upload Flow:
1. Author creates or edits a post
2. Author selects header image file
3. Frontend converts to base64
4. API validates and stores image
5. Header appears immediately in post view

### Supported Use Cases:
- Header image upload during post creation
- Header image updates in post editing
- Header image removal
- Header image display in post lists and detail views
- Blog/article enhancement with visual headers

## 🎨 UI/UX Recommendations

### Header Image Display:
- **Aspect Ratio**: Consider 16:9 or 21:9 for blog headers
- **Responsive Design**: Scale images appropriately on mobile
- **Loading States**: Show placeholder while uploading
- **Fallback**: Graceful handling when no header exists

### Upload Interface:
- **Drag & Drop**: Allow drag and drop for easier uploads
- **Preview**: Show preview before uploading
- **Progress**: Display upload progress for large files
- **Validation**: Real-time validation feedback

---

## 🎉 Ready for Use!

The post header image feature is **production-ready** and fully integrated with the existing post system. Authors can now upload, view, and manage header images for their posts using Base64 encoding, enhancing the visual appeal of their content!

### Quick Start:
1. Build and run the API: `dotnet run --urls="http://localhost:5000"`
2. Test with provided HTTP file: `post-header-image-tests.http`
3. Integrate with frontend using the JavaScript examples above

**Feature Complete! ✨**

### Integration with Existing Features:
- Works seamlessly with existing post CRUD operations
- Compatible with post reactions and comments
- Follows same patterns as profile picture uploads
- Uses existing database schema (no migrations needed)
