window.uploadBlog = async function (blogData, content, apiBaseUrl) {
    let formData = new FormData();
    formData.append("file", new Blob([content], { type: "text/html" }), "blogContent.html");
    formData.append("metadata", new Blob([blogData], { type: "application/json" }), "metadata.json");

    try {
        let response = await fetch(`${apiBaseUrl}/api/Blog/UploadBlog`, {
            method: "POST",
            body: formData
        });

        if (response.ok) {
            console.log("✅ Blog Stored Successfully!");
        } else {
            console.error("❌ Failed to upload blog:", response.status, await response.text());
        }
    } catch (error) {
        console.error("❌ Error uploading blog:", error);
    }
};

