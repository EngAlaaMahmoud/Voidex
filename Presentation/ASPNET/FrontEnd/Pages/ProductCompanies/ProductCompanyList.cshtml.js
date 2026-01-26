// ProductCompanyList - robust grid creation + diagnostics + fallback table render
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

        // modal wrapper (was missing) - create, show, hide
        const mainModal = {
            obj: null,
            create: () => {
                if (mainModalRef.value) {
                    try {
                        mainModal.obj = new bootstrap.Modal(mainModalRef.value, { backdrop: 'static', keyboard: false });
                    } catch (e) {
                        console.warn('[ProductCompany] bootstrap modal creation failed', e);
                        mainModal.obj = null;
                    }
                }
            },
            show: () => {
                if (mainModal.obj) mainModal.obj.show();
                else console.warn('[ProductCompany] mainModal.obj not available to show');
            },
            hide: () => {
                if (mainModal.obj) mainModal.obj.hide();
            }
        };

        const waitForRef = (ref, timeoutMs = 3000, intervalMs = 50) => {
            return new Promise((resolve, reject) => {
                const start = Date.now();
                const timer = setInterval(() => {
                    if (ref.value) {
                        clearInterval(timer);
                        resolve(ref.value);
                        return;
                    }
                    if (Date.now() - start > timeoutMs) {
                        clearInterval(timer);
                        reject(new Error('ref not available within timeout'));
                    }
                }, intervalMs);
            });
        };

        const nameText = {
            obj: null,
            create: () => {
                try {
                    const placeholder = (window.__t && window.__t["Enter Name"]) || 'Enter Name';
                    if (typeof ej !== 'undefined' && ej.inputs && ej.inputs.TextBox) {
                        nameText.obj = new ej.inputs.TextBox({ placeholder });
                        if (nameRef.value) nameText.obj.appendTo(nameRef.value);
                    } else {
                        if (nameRef.value) nameRef.value.setAttribute('placeholder', placeholder);
                        console.warn('[ProductCompany] ej.inputs.TextBox not available, using native input placeholder');
                    }
                } catch (e) {
                    console.warn('nameText.create failed', e);
                }
            },
            refresh: () => { if (nameText.obj) nameText.obj.value = state.name; }
        };

        const services = {
            getMainData: async () => {
                const url = '/ProductCompany/GetProductCompanyList';
                return await AxiosManager.get(url, {});
            },
            createMainData: async (name, street, city, description) =>
                await AxiosManager.post('/ProductCompany/CreateProductCompany', {
                    name, street, city, description, createdById: StorageManager.getUserId()
                }),
            updateMainData: async (id, name, street, city, description) =>
                await AxiosManager.post('/ProductCompany/UpdateProductCompany', {
                    id, name, street, city, description, updatedById: StorageManager.getUserId()
                }),
            deleteMainData: async (id) =>
                await AxiosManager.post('/ProductCompany/DeleteProductCompany', {
                    id, deletedById: StorageManager.getUserId()
                })
        };

        const methods = {
            populateMainData: async () => {
                try {
                    const resp = await services.getMainData();
                    const items = resp?.data?.code === 200 ? (resp.data.content?.data || []) : [];
                    state.mainData = Array.isArray(items) ? items : [];
                    console.debug('[ProductCompany] populateMainData -> items count:', state.mainData.length);
                    if (mainGrid.obj) {
                        mainGrid.refresh();
                        console.debug('[ProductCompany] mainGrid.refresh called');
                    } else {
                        if (mainGridRef.value) tryRenderFallbackTable(mainGridRef.value);
                    }
                } catch (err) {
                    console.error('populateMainData failed', err);
                    state.mainData = [];
                    if (mainGrid.obj) mainGrid.refresh();
                }
            }
        };

        const handler = {
            handleSubmit: async () => {
                state.isSubmitting = true;
                if (!state.name) { state.errors.name = (window.__t && window.__t.Name ? (window.__t.Name + ' required') : 'Name required'); state.isSubmitting = false; return; }
                try {
                    const resp = state.id === '' ? await services.createMainData(state.name, state.street, state.city, state.description)
                        : await services.updateMainData(state.id, state.name, state.street, state.city, state.description);
                    if (resp.data.code === 200) {
                        await methods.populateMainData();
                        if (mainGrid.obj) mainGrid.refresh();
                        mainModal.hide();
                        resetForm();
                    } else {
                        Swal.fire({ icon: 'error', title: (window.__t && window.__t["Save failed"]) || 'Save failed', text: resp.data.message });
                    }
                } catch (e) {
                    Swal.fire({ icon: 'error', title: (window.__t && window.__t["Save failed"]) || 'Error', text: e.message || 'An error occurred' });
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

        // fallback renderer omitted for brevity (unchanged)...
        const tryRenderFallbackTable = (container) => {
            try {
                if (!container) return;
                if (container.querySelector('.fallback-table') && state.mainData.length === 0) {
                    container.innerHTML = '';
                    container.dataset.fallbackRendered = '0';
                    return;
                }
                const data = state.mainData || [];
                let html = '<div class="table-responsive"><table class="table table-bordered table-striped fallback-table"><thead><tr>';
                html += `<th></th>`;
                html += `<th>${(window.__t && window.__t.Name) || 'Name'}</th>`;
                html += `<th>${(window.__t && window.__t.Street) || 'Street'}</th>`;
                html += `<th>${(window.__t && window.__t.City) || 'City'}</th>`;
                html += `<th>${(window.__t && window.__t.Description) || 'Description'}</th>`;
                html += `<th>${(window.__t && window.__t['Created At']) || 'Created At'}</th>`;
                html += '</tr></thead><tbody>';
                for (const row of data) {
                    html += '<tr>';
                    html += '<td><input type="checkbox" /></td>';
                    html += `<td>${row.name ?? ''}</td>`;
                    html += `<td>${row.street ?? ''}</td>`;
                    html += `<td>${row.city ?? ''}</td>`;
                    html += `<td>${row.description ?? ''}</td>`;
                    html += `<td>${row.createdAtUtc ? new Date(row.createdAtUtc).toLocaleString() : ''}</td>`;
                    html += '</tr>';
                }
                html += '</tbody></table></div>';
                container.innerHTML = html;
                container.dataset.fallbackRendered = '1';
                console.debug('[ProductCompany] fallback table rendered, rows:', data.length);
            } catch (e) {
                console.error('[ProductCompany] fallback render failed', e);
            }
        };

        const renderNotAuthorized = (container, details) => {
            try {
                const msg = (window.__t && window.__t["NotAuthorized"]) || 'You are not authorized to view this page.';
                const detailHtml = details ? `<pre class="small text-muted mt-2">${details}</pre>` : '';
                if (container) {
                    container.innerHTML = `<div class="alert alert-warning">${msg}${detailHtml}</div>`;
                    container.dataset.fallbackRendered = '0';
                } else {
                    console.warn('[ProductCompany] cannot render not-authorized - container missing');
                }
            } catch (e) { console.error('renderNotAuthorized error', e); }
        };

        const mainGrid = {
            obj: null,
            create: async () => {
                try {
                    await waitForRef(mainGridRef, 3000).catch((e) => {
                        console.warn('mainGridRef not available quickly:', e);
                    });

                    const container = mainGridRef.value || document.querySelector('.grid-container') || null;
                    if (!container) {
                        console.error('[ProductCompany] mainGrid container not found, aborting grid create');
                        return;
                    }

                    if (!container.style.minHeight) container.style.minHeight = '420px';

                    const isRtl = document.documentElement.dir === 'rtl';

                    if (typeof ej === 'undefined' || !ej.grids || !ej.grids.Grid) {
                        console.warn('[ProductCompany] Syncfusion ej.grids.Grid is not available - rendering fallback table');
                        tryRenderFallbackTable(container);
                        return;
                    }

                    if (container.dataset.fallbackRendered === '1') {
                        container.innerHTML = '';
                        container.dataset.fallbackRendered = '0';
                    }

                    mainGrid.obj = new ej.grids.Grid({
                        height: '420px',
                        enableRtl: isRtl,
                        dataSource: state.mainData,
                        allowPaging: true,
                        pageSettings: { pageSize: 50 },
                        allowSelection: true,
                        selectionSettings: { persistSelection: true, type: 'Single' },
                        columns: [
                            { type: 'checkbox', width: 60 },
                            { field: 'id', isPrimaryKey: true, visible: false },
                            { field: 'name', headerText: (window.__t && window.__t.Name) || 'Name', width: 200 },
                            { field: 'street', headerText: (window.__t && window.__t.Street) || 'Street', width: 200 },
                            { field: 'city', headerText: (window.__t && window.__t.City) || 'City', width: 150 },
                            { field: 'description', headerText: (window.__t && window.__t.Description) || 'Description', width: 200 },
                            { field: 'createdAtUtc', headerText: (window.__t && window.__t["Created At"]) || 'Created At', width: 160, format: 'yyyy-MM-dd HH:mm' }
                        ],
                        toolbar: ['ExcelExport', 'Search',
                            { text: (window.__t && window.__t.Add) || 'Add', id: 'AddCustom', prefixIcon: 'e-add' },
                            { text: (window.__t && window.__t.Edit) || 'Edit', id: 'EditCustom', prefixIcon: 'e-edit' },
                            { text: (window.__t && window.__t.Delete) || 'Delete', id: 'DeleteCustom', prefixIcon: 'e-delete' }],
                        dataBound: function () {
                            try { mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], false); } catch (e) {}
                        },
                        rowSelected: function () {
                            try { mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], true); } catch (e) {}
                        },
                        rowDeselected: function () {
                            try {
                                const selected = mainGrid.obj.getSelectedRecords();
                                const enable = selected && selected.length === 1;
                                mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], enable);
                            } catch (e) {}
                        },
                        toolbarClick: async (args) => {
                            try {
                                if (args.item.id === 'AddCustom') {
                                    resetForm();
                                    state.mainTitle = ((window.__t && window.__t.Add) || 'Add') + ' ' + ((window.__t && window.__t.ProductCompany) || 'Product Company');
                                    mainModal.show();
                                }
                                if (args.item.id === 'EditCustom') {
                                    const recs = mainGrid.obj.getSelectedRecords();
                                    if (!recs.length) {
                                        Swal.fire({ icon: 'warning', title: 'Select a row', text: 'Please select one row to edit.' });
                                        return;
                                    }
                                    const sel = recs[0];
                                    state.id = sel.id; state.name = sel.name; state.street = sel.street; state.city = sel.city; state.description = sel.description;
                                    state.mainTitle = ((window.__t && window.__t.Edit) || 'Edit') + ' ' + ((window.__t && window.__t.ProductCompany) || 'Product Company');
                                    mainModal.show();
                                }
                                if (args.item.id === 'DeleteCustom') {
                                    const recs = mainGrid.obj.getSelectedRecords();
                                    if (!recs.length) {
                                        Swal.fire({ icon: 'warning', title: 'Select a row', text: 'Please select one row to delete.' });
                                        return;
                                    }
                                    const sel = recs[0];
                                    const dialog = await Swal.fire({ icon: 'warning', title: (window.__t && window.__t.Confirm) || 'Confirm', text: (window.__t && window.__t["Delete selected?"]) || 'Delete selected?', showCancelButton: true });
                                    if (dialog && dialog.isConfirmed) {
                                        try {
                                            const resp = await services.deleteMainData(sel.id);
                                            // check server response
                                            if (resp?.data?.code === 200) {
                                                await methods.populateMainData();
                                                mainGrid.refresh();
                                                Swal.fire({ icon: 'success', title: 'Deleted', timer: 1200, showConfirmButton: false });
                                            } else {
                                                Swal.fire({ icon: 'error', title: 'Delete failed', text: resp?.data?.message || 'Server returned an error' });
                                            }
                                        } catch (err) {
                                            console.error('Delete failed', err);
                                            Swal.fire({ icon: 'error', title: 'Delete failed', text: err?.response?.data?.message || err.message || 'An error occurred' });
                                        }
                                    }
                                }
                            } catch (err) {
                                console.error('toolbarClick handler error', err);
                            }
                        }
                    });

                    try {
                        mainGrid.obj.appendTo(container);
                    } catch (e) {
                        console.warn('[ProductCompany] appendTo failed, retrying after nextTick', e);
                        await Vue.nextTick();
                        const fallbackContainer = mainGridRef.value || document.querySelector('.grid-container') || container;
                        mainGrid.obj.appendTo(fallbackContainer);
                    }

                    mainGrid.refresh();
                    if (container.dataset.fallbackRendered === '1') container.dataset.fallbackRendered = '0';
                    console.debug('[ProductCompany] Grid created and refreshed. Data length:', state.mainData.length);
                } catch (e) {
                    console.error('[ProductCompany] mainGrid.create error', e);
                    if (mainGridRef.value) tryRenderFallbackTable(mainGridRef.value);
                }
            },
            refresh: () => {
                if (mainGrid.obj) {
                    mainGrid.obj.setProperties({ dataSource: state.mainData });
                    if (mainGridRef.value && mainGridRef.value.dataset && mainGridRef.value.dataset.fallbackRendered === '1') {
                        mainGridRef.value.dataset.fallbackRendered = '0';
                    }
                } else if (mainGridRef.value) tryRenderFallbackTable(mainGridRef.value);
            }
        };

        Vue.onMounted(async () => {
            try {
                console.debug('[ProductCompany] onMounted start');

                const tokenOk = await SecurityManager.validateToken();
                console.debug('[ProductCompany] validateToken ->', tokenOk);

                const DEV_BYPASS_AUTH = false;
                const authorized = DEV_BYPASS_AUTH ? true : await SecurityManager.authorizePage(['Products']);
                console.debug('[ProductCompany] authorizePage ->', authorized);

                if (!tokenOk) {
                    console.warn('[ProductCompany] token invalid');
                    const container = mainGridRef.value || document.querySelector('.grid-container');
                    if (container) container.innerHTML = '<div class="alert alert-danger">Session expired. Please login again.</div>';
                    return;
                }

                if (!authorized) {
                    console.warn('[ProductCompany] not authorized');
                    const container = mainGridRef.value || document.querySelector('.grid-container');
                    renderNotAuthorized(container, 'Required permission: Products');
                    return;
                }

                // create modal wrapper first
                mainModal.create();

                await methods.populateMainData();
                await new Promise(r => setTimeout(r, 50));
                await mainGrid.create();
                nameText.create();
                if (mainModalRef.value) mainModalRef.value.addEventListener('hidden.bs.modal', resetForm);
                console.debug('[ProductCompany] onMounted finished');
            } catch (e) { console.error(e); }
        });

        Vue.onUnmounted(() => { mainModalRef.value?.removeEventListener('hidden.bs.modal', resetForm); });

        return { mainGridRef, mainModalRef, nameRef, state, handler };
    }
};

Vue.createApp(App).mount('#app');
