const App = {
    setup() {
        const state = Vue.reactive({
            mainData: [],
            deleteMode: false,
            mainTitle: null,
            id: '',
            name: '',
            description: '',
            companyIds: [],
            companyListLookupData: [],
            companyNamesText: '',
            companyRecords: [],
            errors: {
                name: ''
            },
            isSubmitting: false
        });

        const newCompany = Vue.reactive({ name: '', street: '', city: '', description: '' });

        const mainGridRef = Vue.ref(null);
        const mainModalRef = Vue.ref(null);
        const nameRef = Vue.ref(null);
        const companyIdsRef = Vue.ref(null);

        const nameText = {
            obj: null,
            create: () => {
                nameText.obj = new ej.inputs.TextBox({
                    placeholder: 'Enter Name',
                });
                nameText.obj.appendTo(nameRef.value);
            },
            refresh: () => {
                if (nameText.obj) {
                    nameText.obj.value = state.name;
                }
            }
        };

        const addCompanyRecord = () => {
            if (!newCompany.name || !newCompany.name.trim()) {
                Swal.fire({ icon: 'warning', title: 'Name required', text: 'Company name is required.' });
                return;
            }

            const record = { name: newCompany.name.trim(), street: newCompany.street?.trim(), city: newCompany.city?.trim(), description: newCompany.description?.trim() };
            state.companyRecords.push(record);
            // if new company name matches existing product company, add its id to selection immediately
            const match = (state.companyListLookupData || []).find(c => c.name && c.name.toLowerCase() === record.name.toLowerCase());
            if (match) {
                if (!state.companyIds.includes(match.id)) {
                    state.companyIds.push(match.id);
                    setTimeout(() => companyListLookup.refresh(), 50);
                }
            }
            // sync companyNamesText to reflect current selection
            syncCompanyNamesTextFromIds();
            newCompany.name = '';
            newCompany.street = '';
            newCompany.city = '';
            newCompany.description = '';
        };

        const removeCompanyRecord = (idx) => {
            const rec = state.companyRecords[idx];
            state.companyRecords.splice(idx, 1);
            // if removed record matches existing company, remove its id from selection
            const match = (state.companyListLookupData || []).find(c => c.name && rec.name && c.name.toLowerCase() === rec.name.toLowerCase());
            if (match) {
                state.companyIds = (state.companyIds || []).filter(id => id !== match.id);
                setTimeout(() => companyListLookup.refresh(), 50);
            }
            // sync companyNamesText to reflect current selection
            syncCompanyNamesTextFromIds();
        };

        Vue.watch(
            () => state.name,
            (newVal, oldVal) => {
                state.errors.name = '';
                nameText.refresh();
            }
        );

        // when the comma-separated "New Companies" text changes, sync selections
        Vue.watch(
            () => state.companyNamesText,
            (newVal, oldVal) => {
                const names = (newVal || '').split(',').map(s => s.trim()).filter(s => s.length);
                const lookup = state.companyListLookupData || [];
                // determine desired ids from names that match existing product companies
                const desiredIds = [];
                names.forEach(n => {
                    const match = lookup.find(c => c.name && c.name.toLowerCase() === n.toLowerCase());
                    if (match && !desiredIds.includes(match.id)) desiredIds.push(match.id);
                });
                // update state.companyIds to reflect desired ids
                state.companyIds = desiredIds;
                // ensure companyRecords contain entries for selected ids
                desiredIds.forEach(id => {
                    const comp = lookup.find(c => c.id === id);
                    if (comp) {
                        const exists = state.companyRecords.find(r => r._linkedId === id || (r.name && r.name.toLowerCase() === comp.name.toLowerCase()));
                        if (!exists) state.companyRecords.push({ name: comp.name, street: '', city: '', description: '', _linkedId: comp.id });
                    }
                });
                // remove companyRecords that were linked but are no longer present in desiredIds
                state.companyRecords = state.companyRecords.filter(r => !r._linkedId || desiredIds.includes(r._linkedId));
                // refresh multiselect UI
                setTimeout(() => companyListLookup.refresh(), 50);
            }
        );

        const syncCompanyNamesTextFromIds = () => {
            const lookup = state.companyListLookupData || [];
            const names = (state.companyIds || []).map(id => lookup.find(c => c.id === id)?.name).filter(n => n);
            state.companyNamesText = names.join(', ');
        };

        const validateForm = function () {
            state.errors.name = '';

            let isValid = true;

            if (!state.name) {
                state.errors.name = 'Name is required.';
                isValid = false;
            }

            return isValid;
        };

        const resetFormState = () => {
            state.id = '';
            state.name = '';
            state.description = '';
            state.companyIds = [];
            state.companyNamesText = '';
            state.errors = {
                name: ''
            };
        };

        const services = {
            getMainData: async () => {
                try {
                    const response = await AxiosManager.get('/ProductGroup/GetProductGroupList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            createMainData: async (name, description, createdById, companyRecords) => {
                try {
                    const companyNames = (state.companyNamesText || '').split(',').map(s => s.trim()).filter(s => s.length);
                    const response = await AxiosManager.post('/ProductGroup/CreateProductGroup', {
                        name, description, companyIds: state.companyIds, companyNames, companyRecords, createdById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            updateMainData: async (id, name, description, updatedById, companyRecords) => {
                try {
                    const companyNames = (state.companyNamesText || '').split(',').map(s => s.trim()).filter(s => s.length);
                    const response = await AxiosManager.post('/ProductGroup/UpdateProductGroup', {
                        id, name, description, companyIds: state.companyIds, companyNames, companyRecords, updatedById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            deleteMainData: async (id, deletedById) => {
                try {
                    const response = await AxiosManager.post('/ProductGroup/DeleteProductGroup', {
                        id, deletedById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getCompanyListLookupData: async () => {
                try {
                    // load product companies (ProductCompany table) for product-group company selection
                    const response = await AxiosManager.get('/Company/GetProductCompanyList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
        };

        const methods = {
            populateMainData: async () => {
                const response = await services.getMainData();
                state.mainData = response?.data?.content?.data.map(item => ({
                    ...item,
                    companyIds: item.companyIds ?? [],
                    companyNames: item.companyNames ?? '',
                    createdAtUtc: new Date(item.createdAtUtc)
                }));
            },
        };

        const companyListLookup = {
            obj: null,
            create: () => {
                if (state.companyListLookupData && Array.isArray(state.companyListLookupData)) {
                    companyListLookup.obj = new ej.dropdowns.MultiSelect({
                        dataSource: state.companyListLookupData,
                        fields: { value: 'id', text: 'name' },
                        placeholder: 'Select Companies',
                        mode: 'Box',
                    change: (e) => {
                        state.companyIds = e.value;
                        // sync companyRecords: ensure companyRecords contains entries for selected ids if missing
                        const selectedIds = e.value || [];
                        selectedIds.forEach(id => {
                            const existing = state.companyRecords.find(r => r._linkedId === id || (r.name && (state.companyListLookupData.find(c=>c.id===id)?.name || '').toLowerCase() === r.name.toLowerCase()));
                            if (!existing) {
                                const comp = state.companyListLookupData.find(c => c.id === id);
                                if (comp) state.companyRecords.push({ name: comp.name, street: '', city: '', description: '', _linkedId: comp.id });
                            }
                        });
                        // remove companyRecords for ids that are no longer selected (only those with _linkedId)
                        state.companyRecords = state.companyRecords.filter(r => !r._linkedId || selectedIds.includes(r._linkedId));
                        // update the comma-separated textbox to reflect selected company names
                        syncCompanyNamesTextFromIds();
                    }
                    });
                    companyListLookup.obj.appendTo(companyIdsRef.value);
                }
            },
            refresh: () => {
                if (companyListLookup.obj) {
                    companyListLookup.obj.value = state.companyIds;
                }
            }
        };

        const handler = {
            handleSubmit: async function () {
                try {
                    state.isSubmitting = true;
                    await new Promise(resolve => setTimeout(resolve, 300));

                    if (!validateForm()) {
                        return;
                    }

                    // build companyRecords DTOs to send to backend
                    const companyRecordsDto = (state.companyRecords || []).map(c => ({ name: c.name, street: c.street, city: c.city, description: c.description }));

                    const response = state.id === ''
                        ? await services.createMainData(state.name, state.description, StorageManager.getUserId(), companyRecordsDto)
                        : state.deleteMode
                            ? await services.deleteMainData(state.id, StorageManager.getUserId())
                            : await services.updateMainData(state.id, state.name, state.description, StorageManager.getUserId(), companyRecordsDto);

                    if (response.data.code === 200) {
                        await methods.populateMainData();
                        mainGrid.refresh();

                        if (!state.deleteMode) {
                            state.mainTitle = 'Edit Product Group';
                            state.id = response?.data?.content?.data.id ?? '';
                            state.name = response?.data?.content?.data.name ?? '';
                            state.description = response?.data?.content?.data.description ?? '';

                            Swal.fire({
                                icon: 'success',
                                title: state.deleteMode ? 'Delete Successful' : 'Save Successful',
                                text: 'Form will be closed...',
                                timer: 2000,
                                showConfirmButton: false
                            });
                            setTimeout(() => {
                                mainModal.obj.hide();
                            }, 2000);

                        } else {
                            Swal.fire({
                                icon: 'success',
                                title: 'Delete Successful',
                                text: 'Form will be closed...',
                                timer: 2000,
                                showConfirmButton: false
                            });
                            setTimeout(() => {
                                mainModal.obj.hide();
                                resetFormState();
                            }, 2000);
                        }

                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: state.deleteMode ? 'Delete Failed' : 'Save Failed',
                            text: response.data.message ?? 'Please check your data.',
                            confirmButtonText: 'Try Again'
                        });
                    }

                } catch (error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'An Error Occurred',
                        text: error.response?.data?.message ?? 'Please try again.',
                        confirmButtonText: 'OK'
                    });
                } finally {
                    state.isSubmitting = false;
                }
            },
        };

        Vue.onMounted(async () => {
            try {
                await SecurityManager.authorizePage(['ProductGroups']);
                await SecurityManager.validateToken();

                await methods.populateMainData();
                await mainGrid.create(state.mainData);

                // load product companies lookup data and create multi-select
                const compResp = await services.getCompanyListLookupData();
                state.companyListLookupData = compResp?.data?.content?.data ?? [];
                nameText.create();
                mainModal.create();
                companyListLookup.create();
                mainModalRef.value?.addEventListener('hidden.bs.modal', () => {
                    resetFormState();
                });

            } catch (e) {
                console.error('page init error:', e);
            } finally {
                
            }
        });

        Vue.onUnmounted(() => {
            mainModalRef.value?.removeEventListener('hidden.bs.modal', resetFormState);
        });

        const mainGrid = {
            obj: null,
            create: async (dataSource) => {
                mainGrid.obj = new ej.grids.Grid({
                    height: '240px',
                    dataSource: dataSource,
                    allowFiltering: true,
                    allowSorting: true,
                    allowSelection: true,
                    allowGrouping: true,
                    allowTextWrap: true,
                    allowResizing: true,
                    allowPaging: true,
                    allowExcelExport: true,
                    filterSettings: { type: 'CheckBox' },
                    sortSettings: { columns: [{ field: 'createdAtUtc', direction: 'Descending' }] },
                    pageSettings: { currentPage: 1, pageSize: 50, pageSizes: ["10", "20", "50", "100", "200", "All"] },
                    selectionSettings: { persistSelection: true, type: 'Single' },
                    autoFit: true,
                    showColumnMenu: true,
                    gridLines: 'Horizontal',
                    columns: [
                        { type: 'checkbox', width: 60 },
                        {
                            field: 'id', isPrimaryKey: true, headerText: 'Id', visible: false
                        },
                        { field: 'name', headerText: 'Name', width: 200, minWidth: 200 },
                        { field: 'description', headerText: 'Description', width: 400, minWidth: 400 },
                        { field: 'companyNames', headerText: 'Companies', width: 200 },
                        { field: 'createdAtUtc', headerText: 'Created At UTC', width: 150, format: 'yyyy-MM-dd HH:mm' }
                    ],
                    toolbar: [
                        'ExcelExport', 'Search',
                        { type: 'Separator' },
                        { text: 'Add', tooltipText: 'Add', prefixIcon: 'e-add', id: 'AddCustom' },
                        { text: 'Edit', tooltipText: 'Edit', prefixIcon: 'e-edit', id: 'EditCustom' },
                        { text: 'Delete', tooltipText: 'Delete', prefixIcon: 'e-delete', id: 'DeleteCustom' },
                        { type: 'Separator' },
                    ],
                    beforeDataBound: () => { },
                    dataBound: function () {
                        mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], false);
                        mainGrid.obj.autoFitColumns(['name', 'description', 'createdAtUtc']);
                    },
                    excelExportComplete: () => { },
                    rowSelected: () => {
                        if (mainGrid.obj.getSelectedRecords().length == 1) {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], true);
                        } else {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], false);
                        }
                    },
                    rowDeselected: () => {
                        if (mainGrid.obj.getSelectedRecords().length == 1) {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], true);
                        } else {
                            mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], false);
                        }
                    },
                    rowSelecting: () => {
                        if (mainGrid.obj.getSelectedRecords().length) {
                            mainGrid.obj.clearSelection();
                        }
                    },
                    toolbarClick: async (args) => {
                        if (args.item.id === 'MainGrid_excelexport') {
                            mainGrid.obj.excelExport();
                        }

                        if (args.item.id === 'AddCustom') {
                            state.deleteMode = false;
                            state.mainTitle = 'Add Product Group';
                            resetFormState();
                            state.companyIds = [];
                            state.companyNamesText = '';
                            // refresh lookup after reset
                            setTimeout(() => companyListLookup.refresh(), 50);
                            mainModal.obj.show();
                        }

                        if (args.item.id === 'EditCustom') {
                            state.deleteMode = false;
                            if (mainGrid.obj.getSelectedRecords().length) {
                                const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                state.mainTitle = 'Edit Product Group';
                                state.id = selectedRecord.id ?? '';
                                state.name = selectedRecord.name ?? '';
                                state.description = selectedRecord.description ?? '';
                                state.companyIds = selectedRecord.companyIds ?? [];
                                state.companyNamesText = selectedRecord.companyNames ?? '';
                                setTimeout(() => companyListLookup.refresh(), 50);
                                mainModal.obj.show();
                            }
                        }

                        if (args.item.id === 'DeleteCustom') {
                            state.deleteMode = true;
                            if (mainGrid.obj.getSelectedRecords().length) {
                                const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                state.mainTitle = 'Delete Product Group?';
                                state.id = selectedRecord.id ?? '';
                                state.name = selectedRecord.name ?? '';
                                state.description = selectedRecord.description ?? '';
                                mainModal.obj.show();
                            }
                        }
                    }
                });

                mainGrid.obj.appendTo(mainGridRef.value);
            },
            refresh: () => {
                mainGrid.obj.setProperties({ dataSource: state.mainData });
            }
        };

        const mainModal = {
            obj: null,
            create: () => {
                mainModal.obj = new bootstrap.Modal(mainModalRef.value, {
                    backdrop: 'static',
                    keyboard: false
                });
            }
        };

        return {
            mainGridRef,
            mainModalRef,
            nameRef,
            companyIdsRef,
            newCompany,
            state,
            handler,
            addCompanyRecord,
            removeCompanyRecord,
        };
    }
};

Vue.createApp(App).mount('#app');