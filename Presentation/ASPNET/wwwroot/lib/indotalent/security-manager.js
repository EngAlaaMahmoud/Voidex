const SecurityManager = {
    authorizePage: async (requiredRoles) => {
        const userRoles = StorageManager.getUserRoles();
        const isAuthorized = Array.isArray(userRoles) && userRoles.some(role => requiredRoles.includes(role));

        if (!isAuthorized) {
            Swal.fire({
                icon: 'error',
                title: 'Unauthorized',
                text: 'You are being redirected...',
                timer: 2000,
                showConfirmButton: false
            });
            setTimeout(() => {
                window.location.href = '/Accounts/Login';
            }, 2000);
        }

        // IMPORTANT: return boolean so callers can check result
        return !!isAuthorized;
    },

    validateToken: async () => {

        try {
            const response = await AxiosManager.post('/Security/ValidateToken',
                {},
            );

            if (response?.data?.code === 200) {
                return true;
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Token not valid',
                    text: 'You are being redirected...',
                    timer: 2000,
                    showConfirmButton: false
                });
                setTimeout(() => {
                    window.location.href = '/Accounts/Login';
                }, 2000);
                return false;
            }
        } catch (error) {
            Swal.fire({
                icon: 'error',
                title: error?.response?.data?.message || 'Error validating token',
                text: 'You are being redirected...',
                timer: 2000,
                showConfirmButton: false
            });
            setTimeout(() => {
                window.location.href = '/Accounts/Login';
            }, 2000);
            return false;
        }
    }


};