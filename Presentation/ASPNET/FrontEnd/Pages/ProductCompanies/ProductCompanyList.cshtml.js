const App = {
    setup() {
        const state = Vue.reactive({
            mainData: [],
            mainTitle: null,
            id: '',
            name: '',
            street: '',
            city: '',
            description: '',
            errors: { name: '' },
            isSubmitting: false
        });

        const mainGridRef = Vue.ref(null);
        const mainModalRef = Vue.ref(null);
        const nameRef = Vue.ref(null);

        const nameText = {
            obj: null,
            create: () => {
                nameText.obj = new ej.inputs.TextBox({ placeholder: 'Enter Name' });
                nameText.obj.appendTo(nameRef.value);
            },
            refresh: () => { if (nameText.obj) nameText.obj.value = state.name; }
        };

        const services = {
            getMainData: async () => await AxiosManager.get('/ProductCompany/GetProductCompanyList', {}),
            createMainData: async (name, street, city, description) => await AxiosManager.post('/ProductCompany/CreateProductCompany', { name, street, city, description, createdById: StorageManager.getUserId() }),
            updateMainData: async (id, name, street, city, description) => await AxiosManager.post('/ProductCompany/UpdateProductCompany', { id, name, street, city, description, updatedById: StorageManager.getUserId() }),
            deleteMainData: async (id) => await AxiosManager.post('/ProductCompany/DeleteProductCompany', { id, deletedById: StorageManager.getUserId() })
        };

        const methods = {
            populateMainData: async () => {
                const resp = await services.getMainData();
                state.mainData = (resp && resp.data && resp.data.content && resp.data.content.data) ? resp.data.content.data : [];
            }
        };

        const handler = {
            handleSubmit: async () => {
                state.isSubmitting = true;
                if (!state.name) { state.errors.name = 'Name required'; state.isSubmitting = false; return; }
                try {
                    const resp = state.id === '' ? await services.createMainData(state.name, state.street, state.city, state.description) : await services.updateMainData(state.id, state.name, state.street, state.city, state.description);
                    if (resp.data.code === 200) {
                        await methods.populateMainData();
                        mainGrid.refresh();
                        mainModal.obj.hide();
                        resetForm();
                    } else {
                        Swal.fire({ icon: 'error', title: 'Save failed', text: resp.data.message });
                    }
                } catch (e) {
                    Swal.fire({ icon: 'error', title: 'Error', text: e.message || 'An error occurred' });
                } finally { state.isSubmitting = false; }
            }
        };

        const resetForm = () => {
            state.id = '';
            state.name = '';
            state.street = '';
            state.city = '';
            state.description = '';
            state.errors = { name: '' };
        };

        const mainGrid = {
            obj: null,
            create: async () => {
                mainGrid.obj = new ej.grids.Grid({
                    height: '320px',
                    dataSource: state.mainData,
                    allowPaging: true,
                    pageSettings: { pageSize: 50 },
                    columns: [
                        { type: 'checkbox', width: 60 },
                        { field: 'id', isPrimaryKey: true, visible: false },
                        { field: 'name', headerText: 'Name', width: 200 },
                        { field: 'street', headerText: 'Street', width: 200 },
                        { field: 'city', headerText: 'City', width: 150 },
                        { field: 'description', headerText: 'Description', width: 200 },
                        { field: 'createdAtUtc', headerText: 'Created At', width: 160, format: 'yyyy-MM-dd HH:mm' }
                    ],
                    toolbar: [ 'ExcelExport', 'Search', { text: 'Add', id: 'AddCustom', prefixIcon: 'e-add' }, { text: 'Edit', id: 'EditCustom', prefixIcon: 'e-edit' }, { text: 'Delete', id: 'DeleteCustom', prefixIcon: 'e-delete' } ],
                    toolbarClick: async (args) => {
                        if (args.item.id === 'AddCustom') { resetForm(); state.mainTitle = 'Add Product Company'; mainModal.obj.show(); }
                        if (args.item.id === 'EditCustom') {
                            const recs = mainGrid.obj.getSelectedRecords(); if (!recs.length) return; const sel = recs[0]; state.id = sel.id; state.name = sel.name; state.street = sel.street; state.city = sel.city; state.description = sel.description; state.mainTitle = 'Edit Product Company'; mainModal.obj.show();
                        }
                        if (args.item.id === 'DeleteCustom') {
                            const recs = mainGrid.obj.getSelectedRecords();
                            if (!recs.length) return;
                            const sel = recs[0];
                            try {
                                const dialog = await Swal.fire({ icon: 'warning', title: 'Confirm', text: 'Delete selected?', showCancelButton: true });
                                if (dialog && dialog.isConfirmed) {
                                    await services.deleteMainData(sel.id);
                                    await methods.populateMainData();
                                    mainGrid.refresh();
                                }
                            } catch (err) {
                                console.error('Delete confirmation failed', err);
                            }
                        }
                    }
                });
                mainGrid.obj.appendTo(mainGridRef.value);
            },
            refresh: () => { if (mainGrid.obj) mainGrid.obj.setProperties({ dataSource: state.mainData }); }
        };

        Vue.onMounted(async () => {
            try {
                await SecurityManager.authorizePage(['ProductCompanies']);
                await SecurityManager.validateToken();
                await methods.populateMainData();
                await mainGrid.create();
                mainModal.obj = new bootstrap.Modal(mainModalRef.value, { backdrop: 'static', keyboard: false });
                nameText.create();
                if (mainModalRef.value) mainModalRef.value.addEventListener('hidden.bs.modal', resetForm);
            } catch (e) { console.error(e); }
        });

        Vue.onUnmounted(() => { mainModalRef.value?.removeEventListener('hidden.bs.modal', resetForm); });

        return { mainGridRef, mainModalRef, nameRef, state, handler };
    }
};

Vue.createApp(App).mount('#app');
