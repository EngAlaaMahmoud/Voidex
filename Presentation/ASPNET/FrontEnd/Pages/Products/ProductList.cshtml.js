const App = {
    setup() {
        const state = Vue.reactive({
            mainData: [],
            deleteMode: false,
            productGroupListLookupData: [],
            unitMeasureListLookupData: [],
            vatListLookupData: [],
            //taxListLookupData: [],
            mainTitle: null,
            id: '',
            name: '',
            number: '',
            barcode: '',
            internalCode: '',
            gisEgsCode: '',
            companyName: '',
            model: '',
            unitPrice: '',
            discount: '',
            priceAfterDiscount: '',
            serviceFee: '',
            additionalTax: '',
            additionalFee: '',
            description: '',
            productGroupId: null,
            unitMeasureId: null,
            vatId: null,
            productCompanyId: null,
            productCompanyMap: {},
            //taxId: null,
            physical: false,
            errors: {
                name: '',
                unitPrice: '',
                productGroupId: '',
                unitMeasureId: '',
                vatId: '',
                //taxId: ''
            },
            isSubmitting: false
        });

        const mainGridRef = Vue.ref(null);
        const mainModalRef = Vue.ref(null);
        const productGroupIdRef = Vue.ref(null);
        const unitMeasureIdRef = Vue.ref(null);
        const vatIdRef = Vue.ref(null);
        //const taxIdRef = Vue.ref(null);
        const nameRef = Vue.ref(null);
        const numberRef = Vue.ref(null);
        const barcodeRef = Vue.ref(null);
        const unitPriceRef = Vue.ref(null);

        // Define mainModal FIRST to ensure it's available everywhere
        const mainModal = {
            obj: null,
            create: () => {
                if (mainModalRef.value) {
                    mainModal.obj = new bootstrap.Modal(mainModalRef.value, {
                        backdrop: 'static',
                        keyboard: false
                    });
                    console.log('Modal created successfully');
                } else {
                    console.error('Modal element not found');
                }
            },
            show: () => {
                if (mainModal.obj) {
                    mainModal.obj.show();
                } else {
                    console.error('Modal object is null');
                }
            },
            hide: () => {
                if (mainModal.obj) {
                    mainModal.obj.hide();
                }
            }
        };

        // Define lookup objects
        const productGroupListLookup = {
            obj: null,
            create: () => {
                if (state.productGroupListLookupData && Array.isArray(state.productGroupListLookupData) && state.productGroupListLookupData.length > 0) {
                    productGroupListLookup.obj = new ej.dropdowns.DropDownList({
                        dataSource: state.productGroupListLookupData,
                        fields: { value: 'id', text: 'name' },
                        placeholder: 'Select a Product Group',
                        popupHeight: '200px',
                        change: (e) => {
                            state.productGroupId = e.value;
                            console.log('productGroup changed to', e.value);
                            // populate product companies from local map (no network)
                            methods.populateProductCompaniesFromMap(e.value);
                        }
                    });
                    productGroupListLookup.obj.appendTo(productGroupIdRef.value);
                } else {
                    console.warn('ProductGroup list lookup data is not available or invalid.');
                }
            },
            refresh: () => {
                if (productGroupListLookup.obj) {
                    productGroupListLookup.obj.value = state.productGroupId;
                }
            },
        };

        // product company lookup for selected group
        const productCompanyLookup = {
            obj: null,
            create: (dataSource) => {
                // destroy existing first
                productCompanyLookup.destroy();
                const elem = document.getElementById('productCompanyPlaceholder');
                if (!elem) return;
                // create a fresh input element inside placeholder
                elem.innerHTML = '<input id="productCompanyElem" />';
                const list = Array.isArray(dataSource) ? dataSource : [];
                productCompanyLookup.obj = new ej.dropdowns.DropDownList({
                    dataSource: list,
                    fields: { value: 'id', text: 'name' },
                    placeholder: 'Select Product Company',
                    popupHeight: '200px',
                    enabled: list.length > 0,
                    change: (e) => {
                        state.productCompanyId = e.value;
                    }
                });
                productCompanyLookup.obj.appendTo(document.getElementById('productCompanyElem'));
                // set initial value from state
                try { productCompanyLookup.obj.value = state.productCompanyId; } catch (err) { }
            },
            refresh: () => {
                if (productCompanyLookup.obj) {
                    try { productCompanyLookup.obj.value = state.productCompanyId; } catch (err) { }
                }
            }
            ,
            destroy: () => {
                if (productCompanyLookup.obj) {
                    try { productCompanyLookup.obj.destroy(); } catch (err) { }
                    productCompanyLookup.obj = null;
                }
                const elem = document.getElementById('productCompanyPlaceholder');
                if (elem) elem.innerHTML = '';
            }
        };

        const unitMeasureListLookup = {
            obj: null,
            create: () => {
                if (state.unitMeasureListLookupData && Array.isArray(state.unitMeasureListLookupData) && state.unitMeasureListLookupData.length > 0) {
                    unitMeasureListLookup.obj = new ej.dropdowns.DropDownList({
                        dataSource: state.unitMeasureListLookupData,
                        fields: { value: 'id', text: 'name' },
                        placeholder: 'Select a Unit Measure',
                        popupHeight: '200px',
                        change: (e) => {
                            state.unitMeasureId = e.value;
                        }
                    });
                    unitMeasureListLookup.obj.appendTo(unitMeasureIdRef.value);
                } else {
                    console.warn('UnitMeasure list lookup data is not available or invalid.');
                }
            },
            refresh: () => {
                if (unitMeasureListLookup.obj) {
                    unitMeasureListLookup.obj.value = state.unitMeasureId;
                }
            },
        };

        const vatListLookup = {
            obj: null,
            create: () => {
                if (state.vatListLookupData && Array.isArray(state.vatListLookupData) && state.vatListLookupData.length > 0) {
                    vatListLookup.obj = new ej.dropdowns.DropDownList({
                        dataSource: state.vatListLookupData,
                        fields: { value: 'id', text: 'name' },
                        placeholder: 'Select a VAT',
                        popupHeight: '200px',
                        change: (e) => {
                            state.vatId = e.value;
                        }
                    });
                    vatListLookup.obj.appendTo(vatIdRef.value);
                } else {
                    console.warn('VAT list lookup data is not available or invalid.');
                    // Create empty dropdown as fallback
                    vatListLookup.obj = new ej.dropdowns.DropDownList({
                        dataSource: [],
                        fields: { value: 'id', text: 'name' },
                        placeholder: 'No VAT data available',
                        popupHeight: '200px',
                        enabled: false,
                        change: (e) => {
                            state.vatId = e.value;
                        }
                    });
                    vatListLookup.obj.appendTo(vatIdRef.value);
                }
            },
            refresh: () => {
                if (vatListLookup.obj) {
                    vatListLookup.obj.value = state.vatId;
                }
            },
        };

        //const taxListLookup = {
        //    obj: null,
        //    create: () => {
        //        if (state.taxListLookupData && Array.isArray(state.taxListLookupData) && state.taxListLookupData.length > 0) {
        //            taxListLookup.obj = new ej.dropdowns.DropDownList({
        //                dataSource: state.taxListLookupData,
        //                fields: { value: 'id', text: 'name' },
        //                placeholder: 'Select a Tax',
        //                popupHeight: '200px',
        //                change: (e) => {
        //                    state.taxId = e.value;
        //                }
        //            });
        //            taxListLookup.obj.appendTo(taxIdRef.value);
        //        } else {
        //            console.warn('Tax list lookup data is not available or invalid.');
        //            // Create empty dropdown as fallback
        //            taxListLookup.obj = new ej.dropdowns.DropDownList({
        //                dataSource: [],
        //                fields: { value: 'id', text: 'name' },
        //                placeholder: 'No Tax data available',
        //                popupHeight: '200px',
        //                enabled: false,
        //                change: (e) => {
        //                    state.taxId = e.value;
        //                }
        //            });
        //            taxListLookup.obj.appendTo(taxIdRef.value);
        //        }
        //    },
        //    refresh: () => {
        //        if (taxListLookup.obj) {
        //            taxListLookup.obj.value = state.taxId;
        //        }
        //    },
        //};

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

        const numberText = {
            obj: null,
            create: () => {
                numberText.obj = new ej.inputs.TextBox({
                    placeholder: '[auto]',
                    readonly: true
                });
                numberText.obj.appendTo(numberRef.value);
            },
            refresh: () => {
                if (numberText.obj) {
                    numberText.obj.value = state.number;
                }
            },
        };

        const barcodeText = {
            obj: null,
            create: () => {
                barcodeText.obj = new ej.inputs.TextBox({
                    placeholder: 'Enter or Scan Barcode',
                });
                barcodeText.obj.appendTo(barcodeRef.value);
            },
            refresh: () => {
                if (barcodeText.obj) {
                    barcodeText.obj.value = state.barcode;
                }
            }
        };

        const unitPriceNumber = {
            obj: null,
            create: () => {
                unitPriceNumber.obj = new ej.inputs.NumericTextBox({
                    placeholder: 'Enter Unit Price',
                    decimals: 2,
                    format: 'N2',
                    min: 0
                });
                unitPriceNumber.obj.appendTo(unitPriceRef.value);
            },
            refresh: () => {
                if (unitPriceNumber.obj) {
                    unitPriceNumber.obj.value = state.unitPrice;
                }
            }
        };

        const validateForm = function () {
            state.errors.name = '';
            state.errors.unitPrice = '';
            state.errors.productGroupId = '';
            state.errors.unitMeasureId = '';
            state.errors.vatId = '';
            //state.errors.taxId = '';

            let isValid = true;

            if (!state.name) {
                state.errors.name = 'Name is required.';
                isValid = false;
            }
            if (!state.unitPrice) {
                state.errors.unitPrice = 'Unit price is required.';
                isValid = false;
            }
            if (!state.productGroupId) {
                state.errors.productGroupId = 'Product group is required.';
                isValid = false;
            }
            if (!state.unitMeasureId) {
                state.errors.unitMeasureId = 'Unit measure is required.';
                isValid = false;
            }

            return isValid;
        };

        const resetFormState = () => {
            state.id = '';
            state.name = '';
            state.number = '';
            state.barcode = '';
            state.unitPrice = '';
            state.description = '';
            state.productGroupId = null;
            state.productCompanyId = null;
            state.unitMeasureId = null;
            state.vatId = null;
            //state.taxId = null;
            state.physical = false;
            state.errors = {
                name: '',
                unitPrice: '',
                productGroupId: '',
                unitMeasureId: '',
                vatId: '',
                //taxId: ''
            };
        };

        const services = {
            getMainData: async () => {
                try {
                    const response = await AxiosManager.get('/Product/GetProductList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            createMainData: async (name, barcode, unitPrice, description, productGroupId, unitMeasureId, vatId, physical, createdById) => {
                try {
                    const response = await AxiosManager.post('/Product/CreateProduct', {
                        name, barcode, unitPrice, description, productGroupId, productCompanyId: state.productCompanyId, unitMeasureId, vatId, physical, createdById,
                        internalCode: state.internalCode,
                        gisEgsCode: state.gisEgsCode,
                        companyName: state.companyName,
                        model: state.model,
                        discount: state.discount,
                        priceAfterDiscount: state.priceAfterDiscount,
                        serviceFee: state.serviceFee,
                        additionalTax: state.additionalTax,
                        additionalFee: state.additionalFee
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            updateMainData: async (id, name, barcode, unitPrice, description, productGroupId, unitMeasureId, vatId, physical, updatedById) => {
                try {
                    const response = await AxiosManager.post('/Product/UpdateProduct', {
                        id, name, barcode, unitPrice, description, productGroupId, productCompanyId: state.productCompanyId, unitMeasureId, vatId, physical, updatedById,
                        internalCode: state.internalCode,
                        gisEgsCode: state.gisEgsCode,
                        companyName: state.companyName,
                        model: state.model,
                        discount: state.discount,
                        priceAfterDiscount: state.priceAfterDiscount,
                        serviceFee: state.serviceFee,
                        additionalTax: state.additionalTax,
                        additionalFee: state.additionalFee
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            deleteMainData: async (id, deletedById) => {
                try {
                    const response = await AxiosManager.post('/Product/DeleteProduct', {
                        id, deletedById
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getProductGroupListLookupData: async () => {
                try {
                    const response = await AxiosManager.get('/ProductGroup/GetProductGroupList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getProductCompaniesByGroup: async (groupId) => {
                try {
                    const response = await AxiosManager.get('/ProductGroup/GetProductCompaniesByGroup?groupId=' + groupId, {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getUnitMeasureListLookupData: async () => {
                try {
                    const response = await AxiosManager.get('/UnitMeasure/GetUnitMeasureList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            getVatListLookupData: async () => {
                try {
                    const response = await AxiosManager.get('/Vat/GetVatList', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
            //getTaxListLookupData: async () => {
            //    try {
            //        const response = await AxiosManager.get('/Tax/GetTaxList', {});
            //        return response;
            //    } catch (error) {
            //        throw error;
            //    }
            //}
        };

        const methods = {
            populateProductGroupListLookupData: async () => {
                const response = await services.getProductGroupListLookupData();
                state.productGroupListLookupData = response?.data?.content?.data;
                // also clear product company selection
                state.productCompanyId = null;
            },
            populateUnitMeasureListLookupData: async () => {
                const response = await services.getUnitMeasureListLookupData();
                state.unitMeasureListLookupData = response?.data?.content?.data;
            },
            populateVatListLookupData: async () => {
                const response = await services.getVatListLookupData();
                state.vatListLookupData = response?.data?.content?.data;
            },

            populateProductCompaniesByGroup: async (groupId) => {
                if (!groupId) return;
                try {
                    const resp = await services.getProductCompaniesByGroup(groupId);
                    console.log('GetProductCompaniesByGroup response', resp);
                    const items = resp?.data?.content?.data ?? [];
                    if (!items || !items.length) {
                        console.warn('No product companies returned for group', groupId, items);
                    }
                    // render dropdown placeholder
                    const placeholder = document.getElementById('productCompanyPlaceholder');
                    if (placeholder) placeholder.innerHTML = '';
                    // fallback: if API returned empty, try to build items from loaded productGroupListLookupData (companyIds + companyNames)
                    let finalItems = items;
                    if ((!finalItems || !finalItems.length) && Array.isArray(state.productGroupListLookupData)) {
                        const grp = state.productGroupListLookupData.find(g => g.id === groupId);
                        if (grp && Array.isArray(grp.companyIds) && grp.companyIds.length) {
                            const names = (grp.companyNames || '').split(',').map(s => s.trim()).filter(s => s.length);
                            finalItems = grp.companyIds.map((id, idx) => ({ id, name: names[idx] ?? '' }));
                        }
                    }

                    productCompanyLookup.create(finalItems || []);
                } catch (err) {
                    console.error('failed loading product companies for group', err);
                }
            },

            populateProductCompaniesFromMap: async (groupId) => {
                if (!groupId) return;
                const mapping = state.productCompanyMap || {};
                const items = mapping[groupId] || [];
                if (!items || !items.length) {
                    // try to build from productGroupListLookupData
                    const grp = state.productGroupListLookupData.find(g => g.id === groupId);
                    if (grp && Array.isArray(grp.companyIds) && grp.companyIds.length) {
                        const names = (grp.companyNames || '').split(',').map(s => s.trim()).filter(s => s.length);
                        productCompanyLookup.create(grp.companyIds.map((id, idx) => ({ id, name: names[idx] ?? '' })));
                        return;
                    }
                }
                productCompanyLookup.create(items);
            },
            //populateTaxListLookupData: async () => {
            //    const response = await services.getTaxListLookupData();
            //    state.taxListLookupData = response?.data?.content?.data;
            //},
            populateMainData: async () => {
                const response = await services.getMainData();
                state.mainData = response?.data?.content?.data.map(item => ({
                    ...item,
                    internalCode: item.internalCode ?? item.InternalCode ?? '',
                    gisEgsCode: item.gisEgsCode ?? item.GisEgsCode ?? '',
                    companyName: item.companyName ?? item.CompanyName ?? '',
                    model: item.model ?? item.Model ?? '',
                    discount: item.discount ?? item.Discount ?? null,
                    priceAfterDiscount: item.priceAfterDiscount ?? item.PriceAfterDiscount ?? null,
                    serviceFee: item.serviceFee ?? item.ServiceFee ?? null,
                    additionalTax: item.additionalTax ?? item.AdditionalTax ?? null,
                    additionalFee: item.additionalFee ?? item.AdditionalFee ?? null,
                    createdAtUtc: new Date(item.createdAtUtc)
                }));
            },
            handleFormSubmit: async () => {
                state.isSubmitting = true;
                await new Promise(resolve => setTimeout(resolve, 200));

                if (!validateForm()) {
                    state.isSubmitting = false;
                    return;
                }

                try {
                    const response = state.id === ''
                        ? await services.createMainData(state.name, state.barcode, state.unitPrice, state.description, state.productGroupId, state.unitMeasureId, state.vatId, state.physical, StorageManager.getUserId())
                        : state.deleteMode
                            ? await services.deleteMainData(state.id, StorageManager.getUserId())
                            : await services.updateMainData(state.id, state.name, state.barcode, state.unitPrice, state.description, state.productGroupId, state.unitMeasureId, state.vatId,  state.physical, StorageManager.getUserId());

                    if (response.data.code === 200) {
                        await methods.populateMainData();
                        mainGrid.refresh();

                        if (!state.deleteMode) {
                            state.mainTitle = 'Edit Product';
                            state.id = response?.data?.content?.data.id ?? '';
                            state.number = response?.data?.content?.data.number ?? '';
                            state.name = response?.data?.content?.data.name ?? '';
                            state.barcode = response?.data?.content?.data.barcode ?? '';
                            state.unitPrice = response?.data?.content?.data.unitPrice ?? '';
                            state.description = response?.data?.content?.data.description ?? '';
                            state.productGroupId = response?.data?.content?.data.productGroupId ?? '';
                            state.unitMeasureId = response?.data?.content?.data.unitMeasureId ?? '';
                            state.vatId = response?.data?.content?.data.vatId ?? '';
                            //state.taxId = response?.data?.content?.data.taxId ?? '';
                            state.physical = response?.data?.content?.data.physical ?? false;

                            Swal.fire({
                                icon: 'success',
                                title: 'Save Successful',
                                timer: 1000,
                                showConfirmButton: false
                            });
                        } else {
                            Swal.fire({
                                icon: 'success',
                                title: 'Delete Successful',
                                text: 'Form will be closed...',
                                timer: 2000,
                                showConfirmButton: false
                            });
                            setTimeout(() => {
                                mainModal.hide();
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
            }
        };

        const handler = {
            handleSubmit: methods.handleFormSubmit
        };

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
                    groupSettings: { columns: ['productGroupName'] },
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
                        { field: 'number', headerText: 'Number', width: 150, minWidth: 150 },
                        { field: 'barcode', headerText: 'Barcode', width: 120, minWidth: 120 },
                        { field: 'internalCode', headerText: 'Internal Code', width: 120 },
                        { field: 'gisEgsCode', headerText: 'GIS / EGS Code', width: 120 },
                        { field: 'companyName', headerText: 'Company', width: 150 },
                        { field: 'productCompanyName', headerText: 'Product Company', width: 150 },
                        { field: 'model', headerText: 'Model', width: 150 },
                        { field: 'unitPrice', headerText: 'Unit Price', width: 100, format: 'N2' },
                        { field: 'discount', headerText: 'Discount', width: 100, format: 'N2' },
                        { field: 'priceAfterDiscount', headerText: 'Price After Discount', width: 120, format: 'N2' },
                        { field: 'serviceFee', headerText: 'Service Fee', width: 120, format: 'N2' },
                        { field: 'additionalTax', headerText: 'Additional Tax', width: 120, format: 'N2' },
                        { field: 'productGroupName', headerText: 'Product Group', width: 150 },
                        { field: 'unitMeasureName', headerText: 'Unit Measure', width: 150 },
                        { field: 'vatName', headerText: 'VAT', width: 150 },
                        //{ field: 'taxName', headerText: 'Tax', width: 150 },
                        { field: 'physical', headerText: 'Physical', width: 150, displayAsCheckBox: true, type: 'boolean' },
                        { field: 'createdAtUtc', headerText: 'Created At UTC', width: 150, format: 'yyyy-MM-dd HH:mm' }
                    ],
                    toolbar: [
                        'ExcelExport', 'Search',
                        { type: 'Separator' },
                        { text: 'Add', tooltipText: 'Add', prefixIcon: 'e-add', id: 'AddCustom' },
                        { text: 'Edit', tooltipText: 'Edit', prefixIcon: 'e-edit', id: 'EditCustom' },
                        { text: 'Delete', tooltipText: 'Delete', prefixIcon: 'e-delete', id: 'DeleteCustom' },
                    ],
                    beforeDataBound: () => { },
                    dataBound: function () {
                        mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], false);
                        mainGrid.obj.autoFitColumns(['name', 'number', 'barcode', 'unitPrice', 'productGroupName', 'unitMeasureName', 'vatName', 'physical', 'createdAtUtc']);
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
                            state.mainTitle = 'Add Product';
                            resetFormState();
                            // ensure any previous product company dropdown is cleared
                            productCompanyLookup.destroy();
                            mainModal.show();
                        }

                        if (args.item.id === 'EditCustom') {
                            state.deleteMode = false;
                            if (mainGrid.obj.getSelectedRecords().length) {
                                const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                state.mainTitle = 'Edit Product';
                                state.id = selectedRecord.id ?? '';
                                state.number = selectedRecord.number ?? '';
                                state.name = selectedRecord.name ?? '';
                                state.barcode = selectedRecord.barcode ?? '';
                                state.unitPrice = selectedRecord.unitPrice ?? '';
                                state.description = selectedRecord.description ?? '';
                            state.productGroupId = selectedRecord.productGroupId ?? '';
                            state.productCompanyId = selectedRecord.productCompanyId ?? null;
                                state.unitMeasureId = selectedRecord.unitMeasureId ?? '';
                                state.vatId = selectedRecord.vatId ?? '';
                                //state.taxId = selectedRecord.taxId ?? '';
                                state.physical = selectedRecord.physical ?? false;
                                // populate product companies for this group then show modal so dropdown appears with selected value
                                try {
                                    await methods.populateProductCompaniesByGroup(state.productGroupId);
                                } catch (e) {
                                    console.error('failed to populate product companies on edit', e);
                                }
                                mainModal.show();
                            }
                        }

                        if (args.item.id === 'DeleteCustom') {
                            state.deleteMode = true;
                            if (mainGrid.obj.getSelectedRecords().length) {
                                const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                state.mainTitle = 'Delete Product?';
                                state.id = selectedRecord.id ?? '';
                                state.number = selectedRecord.number ?? '';
                                state.name = selectedRecord.name ?? '';
                                state.barcode = selectedRecord.barcode ?? '';
                                state.unitPrice = selectedRecord.unitPrice ?? '';
                                state.description = selectedRecord.description ?? '';
                                state.productGroupId = selectedRecord.productGroupId ?? '';
                                state.unitMeasureId = selectedRecord.unitMeasureId ?? '';
                                state.vatId = selectedRecord.vatId ?? '';
                                //state.taxId = selectedRecord.taxId ?? '';
                                state.physical = selectedRecord.physical ?? false;
                                mainModal.show();
                            }
                        }
                    }
                });

                mainGrid.obj.appendTo(mainGridRef.value);
            },
            refresh: () => {
                if (mainGrid.obj) {
                    mainGrid.obj.setProperties({ dataSource: state.mainData });
                }
            }
        };

        Vue.watch(
            () => state.name,
            (newVal, oldVal) => {
                state.errors.name = '';
                if (nameText.obj) {
                    nameText.refresh();
                }
            }
        );

        Vue.watch(
            () => state.number,
            (newVal, oldVal) => {
                if (numberText.obj) {
                    numberText.refresh();
                }
            }
        );

        Vue.watch(
            () => state.barcode,
            (newVal, oldVal) => {
                if (barcodeText.obj) {
                    barcodeText.refresh();
                }
            }
        );

        Vue.watch(
            () => state.unitPrice,
            (newVal, oldVal) => {
                state.errors.unitPrice = '';
                if (unitPriceNumber.obj) {
                    unitPriceNumber.refresh();
                }
            }
        );

        Vue.watch(
            () => state.productGroupId,
            (newVal, oldVal) => {
                state.errors.productGroupId = '';
                if (productGroupListLookup.obj) {
                    productGroupListLookup.refresh();
                }
            }
        );

        Vue.watch(
            () => state.unitMeasureId,
            (newVal, oldVal) => {
                state.errors.unitMeasureId = '';
                if (unitMeasureListLookup.obj) {
                    unitMeasureListLookup.refresh();
                }
            }
        );

        Vue.watch(
            () => state.vatId,
            (newVal, oldVal) => {
                state.errors.vatId = '';
                if (vatListLookup.obj) {
                    vatListLookup.refresh();
                }
            }
        );

        //Vue.watch(
        //    () => state.taxId,
        //    (newVal, oldVal) => {
        //        state.errors.taxId = '';
        //        if (taxListLookup.obj) {
        //            taxListLookup.refresh();
        //        }
        //    }
        //);

        Vue.onMounted(async () => {
            try {
                await SecurityManager.authorizePage(['Products']);
                await SecurityManager.validateToken();

                // Initialize modal FIRST before anything else
                mainModal.create();

                // Load main data and create grid
                await methods.populateMainData();
                await mainGrid.create(state.mainData);

                // Load lookup data
                await methods.populateProductGroupListLookupData();
                productGroupListLookup.create();
                console.log('productGroupListLookupData', state.productGroupListLookupData);
                // build local productCompanyMap from loaded productGroupListLookupData
                state.productCompanyMap = {};
                state.productGroupListLookupData.forEach(g => {
                    if (g && Array.isArray(g.companyIds) && g.companyIds.length) {
                        const names = (g.companyNames || '').split(',').map(s => s.trim()).filter(s => s.length);
                        state.productCompanyMap[g.id] = g.companyIds.map((id, idx) => ({ id, name: names[idx] ?? '' }));
                    }
                });
                console.log('productCompanyMap', state.productCompanyMap);

                await methods.populateUnitMeasureListLookupData();
                unitMeasureListLookup.create();

                await methods.populateVatListLookupData();
                vatListLookup.create();

                //await methods.populateTaxListLookupData();
                //taxListLookup.create();

                // Create form controls
                nameText.create();
                numberText.create();
                barcodeText.create();
                unitPriceNumber.create();

                // Add modal event listener
                if (mainModalRef.value) {
                    mainModalRef.value.addEventListener('hidden.bs.modal', () => {
                        resetFormState();
                    });
                }

                console.log('Page initialization completed successfully');

            } catch (e) {
                console.error('Page init error:', e);
                Swal.fire({
                    icon: 'error',
                    title: 'Initialization Error',
                    text: 'Failed to initialize the page. Please refresh and try again.',
                    confirmButtonText: 'OK'
                });
            }
        });

        Vue.onUnmounted(() => {
            if (mainModalRef.value) {
                mainModalRef.value.removeEventListener('hidden.bs.modal', resetFormState);
            }
        });

        return {
            mainGridRef,
            mainModalRef,
            productGroupIdRef,
            unitMeasureIdRef,
            vatIdRef,
            //taxIdRef,
            nameRef,
            numberRef,
            barcodeRef,
            unitPriceRef,
            state,
            handler,
        };
    }
};

Vue.createApp(App).mount('#app');