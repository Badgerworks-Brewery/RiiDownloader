// Function to fetch data from GitHub API
async function fetchGitHubData(endpoint) {
    const response = await fetch(`https://api.github.com/repos/your-repo/${endpoint}`);
    if (!response.ok) {
        throw new Error('Network response was not ok');
    }
    return await response.json();
}

export { fetchGitHubData };