(function () {
    async function parseResponse(response) {
        const payload = await response.json();
        return {
            success: Boolean(payload.success),
            message: payload.message || '',
            userId: Number(payload.userId || 0),
            username: payload.username || ''
        };
    }

    window.authManager = {
        async login(username, password) {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                credentials: 'same-origin',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username, password })
            });

            return parseResponse(response);
        },
        async logout() {
            await fetch('/api/auth/logout', {
                method: 'POST',
                credentials: 'same-origin'
            });
        }
    };
})();
