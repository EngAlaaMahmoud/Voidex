// (full file replacement)
const App = {
    setup() {
        const state = Vue.reactive({
            mainData: [],
            deleteMode: false,
            productGroupListLookupData: [],
            unitMeasureListLookupData: [],
            vatListLookupData: [],
            taxListData: [],
            mainTitle: null,
            id: '',
            name: '',
            number: '',
            barcode: '',
            internalCode: '',
            gisEgsCode: '',
            model: '',
            unitPrice: 0,
            discount: 0,
            priceAfterDiscount: 0,
            description: '',
            productGroupId: null,
            unitMeasureId: null,
            vatId: null,
            productCompanyId: null,
            productCompanyMap: {},
            physical: false,
            productTaxes: [],
            errors: {
                name: '',
                unitPrice: '',
                productGroupId: '',
                unitMeasureId: '',
                vatId: '',
            },
            isSubmitting: false
        });

        const mainGridRef = Vue.ref(null);
        const mainModalRef = Vue.ref(null);
        const productGroupIdRef = Vue.ref(null);
        const unitMeasureIdRef = Vue.ref(null);
        const nameRef = Vue.ref(null);
        const numberRef = Vue.ref(null);
        const barcodeRef = Vue.ref(null);
        const unitPriceRef = Vue.ref(null);

        const formatCurrency = (value) => {
            if (!value && value !== 0) return '0.00';
            return parseFloat(value || 0).toLocaleString('en-US', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
        };

        const getUniqueMainCodes = () => {
            if (!state.taxListData || !state.taxListData.length) return [];
            const mainCodes = [...new Set(state.taxListData.map(tax => tax.mainCode))];
            return mainCodes.filter(code => code);
        };

        const getSubCodesByMainCode = (mainCode) => {
            if (!mainCode || !state.taxListData || !state.taxListData.length) return [];
            return state.taxListData
                .filter(tax => tax.mainCode === mainCode)
                .map(tax => ({
                    code: tax.subCode,
                    description: tax.description || tax.taxCategoryName,
                    percentage: tax.percentage,
                    taxCategoryName: tax.taxCategoryName,
                    id: tax.id
                }));
        };

        const addTaxRow = () => {
            state.productTaxes.push({
                id: null,
                taxMainCode: '',
                taxSubCode: '',
                taxCategoryName: '',
                percentage: 0,
                taxValue: 0,
                taxId: null
            });
        };

        const removeTaxRow = (index) => {
            state.productTaxes.splice(index, 1);
            calculateTaxValues();
        };

        const onTaxMainCodeChange = (index) => {
            const taxRow = state.productTaxes[index];
            taxRow.taxSubCode = '';
            taxRow.taxCategoryName = '';
            taxRow.percentage = 0;
            taxRow.taxValue = 0;
            taxRow.taxId = null;
        };

        const onTaxSubCodeChange = (index) => {
            const taxRow = state.productTaxes[index];
            const mainCode = taxRow.taxMainCode;
            const subCode = taxRow.taxSubCode;

            if (mainCode && subCode) {
                const tax = state.taxListData.find(t =>
                    t.mainCode === mainCode && t.subCode === subCode
                );

                if (tax) {
                    taxRow.taxCategoryName = tax.taxCategoryName || tax.taxType || '';
                    taxRow.percentage = tax.percentage || 0;
                    taxRow.taxId = tax.id;
                    calculateTaxValue(index);
                }
            }
        };

        const calculateTaxValue = (index) => {
            const taxRow = state.productTaxes[index];
            const basePrice = (state.priceAfterDiscount && Number(state.priceAfterDiscount) > 0) ? Number(state.priceAfterDiscount) : Number(state.unitPrice || 0);
            const percentage = parseFloat(taxRow.percentage) || 0;

            if (basePrice > 0 && percentage > 0) {
                taxRow.taxValue = (basePrice * percentage) / 100;
            } else {
                taxRow.taxValue = Number(taxRow.taxValue || 0);
            }
        };

        const calculateTaxValues = () => {
            state.productTaxes.forEach((taxRow, index) => {
                calculateTaxValue(index);
            });
        };

        const calculateTotalTaxPercentage = () => {
            return state.productTaxes.reduce((total, tax) => {
                return total + (parseFloat(tax.percentage) || 0);
            }, 0);
        };

        const calculateTotalTaxValue = () => {
            return state.productTaxes.reduce((total, tax) => {
                return total + (parseFloat(tax.taxValue) || 0);
            }, 0);
        };

        const calculatePriceAfterTaxes = () => {
            const basePrice = state.priceAfterDiscount > 0 ? state.priceAfterDiscount : state.unitPrice;
            return basePrice + calculateTotalTaxValue();
        };

        const mainModal = {
            obj: null,
            create: () => {
                if (mainModalRef.value) {
                    mainModal.obj = new bootstrap.Modal(mainModalRef.value, {
                        backdrop: 'static',
                        keyboard: false
                    });
                }
            },
            show: () => {
                if (mainModal.obj) mainModal.obj.show();
            },
            hide: () => {
                if (mainModal.obj) mainModal.obj.hide();
            }
        };

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
                            if (methods && typeof methods.populateProductCompaniesFromMap === 'function') {
                                methods.populateProductCompaniesFromMap(e.value);
                            } else if (methods && typeof methods.populateProductCompaniesByGroup === 'function') {
                                methods.populateProductCompaniesByGroup(e.value);
                            } else {
                                console.warn('No product company population method available');
                            }
                        }
                    });
                    productGroupListLookup.obj.appendTo(productGroupIdRef.value);
                }
            },
            refresh: () => {
                if (productGroupListLookup.obj) {
                    productGroupListLookup.obj.value = state.productGroupId;
                }
            },
        };

        const productCompanyLookup = {
            obj: null,
            create: (dataSource) => {
                productCompanyLookup.destroy();
                const elem = document.getElementById('productCompanyPlaceholder');
                if (!elem) return;
                elem.innerHTML = '<input id="productCompanyElem" />';
                const list = Array.isArray(dataSource) ? dataSource : [];
                productCompanyLookup.obj = new ej.dropdowns.DropDownList({
                    dataSource: list,
                    fields: { value: 'id', text: 'name' },
                    placeholder: 'Select Product Company',
                    popupHeight: '200px',
                    enabled: true,
                    change: (e) => {
                        state.productCompanyId = e.value;
                    }
                });
                productCompanyLookup.obj.appendTo(document.getElementById('productCompanyElem'));
                try { productCompanyLookup.obj.value = state.productCompanyId; } catch (err) { }
                const clearBtn = document.getElementById('productCompanyClearBtn');
                if (clearBtn) {
                    clearBtn.onclick = () => {
                        try { productCompanyLookup.obj.value = null; } catch { }
                        state.productCompanyId = null;
                    };
                }
            },
            refresh: () => {
                if (productCompanyLookup.obj) {
                    try { productCompanyLookup.obj.value = state.productCompanyId; } catch (err) { }
                }
            },
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
                }
            },
            refresh: () => {
                if (unitMeasureListLookup.obj) {
                    unitMeasureListLookup.obj.value = state.unitMeasureId;
                }
            },
        };

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
                const el = unitPriceRef.value;
                if (el) unitPriceNumber.obj.appendTo(el);
            },
            refresh: () => {
                if (unitPriceNumber.obj) {
                    unitPriceNumber.obj.value = state.unitPrice;
                }
            }
        };

        const calculatePriceAfterDiscount = () => {
            const p = Number(state.unitPrice) || 0;
            const d = Number(state.discount) || 0;
            return Math.max(0, p - d);
        };

        const validateForm = function () {
            state.errors.name = '';
            state.errors.unitPrice = '';
            state.errors.productGroupId = '';
            state.errors.unitMeasureId = '';
            state.errors.vatId = '';

            let isValid = true;

            if (!state.name) {
                state.errors.name = 'Name is required.';
                isValid = false;
            }
            if (state.unitPrice === null || state.unitPrice === undefined || state.unitPrice === '') {
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
            state.unitPrice = 0;
            state.description = '';
            state.productGroupId = null;
            state.productCompanyId = null;
            state.unitMeasureId = null;
            state.vatId = null;
            state.physical = true;
            state.productTaxes = [];
            state.discount = 0;
            state.priceAfterDiscount = 0;
            state.errors = {
                name: '',
                unitPrice: '',
                productGroupId: '',
                unitMeasureId: '',
                vatId: '',
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
            createMainData: async (name, barcode, unitPrice, description,
                productGroupId, unitMeasureId, vatId, physical,
                createdById, productTaxes) => {
                try {
                    const response = await AxiosManager.post('/Product/CreateProduct', {
                        name,
                        barcode,
                        unitPrice,
                        description,
                        productGroupId,
                        productCompanyId: state.productCompanyId,
                        unitMeasureId,
                        vatId,
                        physical,
                        createdById,
                        internalCode: state.internalCode,
                        gisEgsCode: state.gisEgsCode,
                        model: state.model,
                        discount: state.discount,
                        priceAfterDiscount: state.priceAfterDiscount,
                        productTaxes: productTaxes
                    });
                    return response;
                } catch (error) {
                    throw error;
                }
            },

            updateMainData: async (id, name, barcode, unitPrice, description,
                productGroupId, unitMeasureId, vatId, physical,
                updatedById, productTaxes) => {
                try {
                    const response = await AxiosManager.post('/Product/UpdateProduct', {
                        id,
                        name,
                        barcode,
                        unitPrice,
                        description,
                        productGroupId,
                        productCompanyId: state.productCompanyId,
                        unitMeasureId,
                        vatId,
                        physical,
                        updatedById,
                        internalCode: state.internalCode,
                        gisEgsCode: state.gisEgsCode,
                        model: state.model,
                        discount: state.discount,
                        priceAfterDiscount: state.priceAfterDiscount,
                        productTaxes: productTaxes
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
            getTaxListData: async () => {
                try {
                    const response = await AxiosManager.get('/Tax/GetTaxList', {});
                    return response;
                } catch (error) {
                    console.error('Failed to load tax list:', error);
                    throw error;
                }
            },
        };

        const methods = {
            populateProductGroupListLookupData: async () => {
                const response = await services.getProductGroupListLookupData();
                state.productGroupListLookupData = response?.data?.content?.data;
                state.productCompanyId = null;
            },
            populateUnitMeasureListLookupData: async () => {
                const response = await services.getUnitMeasureListLookupData();
                state.unitMeasureListLookupData = response?.data?.content?.data;
            },

            populateProductCompaniesFromMap: async (groupId) => {
                if (!groupId) return;
                const mapping = state.productCompanyMap || {};
                const items = mapping[groupId] || [];
                if ((!items || !items.length) && Array.isArray(state.productGroupListLookupData)) {
                    const grp = state.productGroupListLookupData.find(g => g.id === groupId);
                    if (grp && Array.isArray(grp.companyIds) && grp.companyIds.length) {
                        const names = (grp.companyNames || '').split(',').map(s => s.trim()).filter(s => s.length);
                        productCompanyLookup.create(grp.companyIds.map((id, idx) => ({ id, name: names[idx] ?? '' })));
                        return;
                    }
                }
                productCompanyLookup.create(items);
            },

            populateProductCompaniesByGroup: async (groupId) => {
                if (!groupId) return;
                try {
                    const resp = await services.getProductCompaniesByGroup(groupId);
                    const items = resp?.data?.content?.data ?? [];
                    const placeholder = document.getElementById('productCompanyPlaceholder');
                    if (placeholder) placeholder.innerHTML = '';
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

            populateTaxListData: async () => {
                try {
                    const response = await services.getTaxListData();
                    state.taxListData = response?.data?.content?.data || [];
                    console.log('Loaded tax list:', state.taxListData.length, 'items');
                } catch (error) {
                    console.error('Failed to load tax list:', error);
                    state.taxListData = [];
                }
            },

            populateMainData: async () => {
                const response = await services.getMainData();
                state.mainData = response?.data?.content?.data.map(item => {
                    const rawTaxes = item.productTaxes || item.ProductTaxes || [];
                    const normalizedTaxes = Array.isArray(rawTaxes) ? rawTaxes.map(t => ({
                        taxId: t.taxId ?? t.TaxId ?? null,
                        taxMainCode: t.mainCode ?? t.MainCode ?? t.taxMainCode ?? '',
                        taxSubCode: t.subCode ?? t.SubCode ?? t.taxSubCode ?? '',
                        taxCategoryName: t.taxCategoryName ?? t.TaxCategoryName ?? '',
                        description: t.description ?? t.Description ?? '',
                        percentage: Number(t.percentage ?? t.Percentage ?? 0),
                        taxValue: Number(t.taxValue ?? t.TaxValue ?? 0)
                    })) : [];

                    const t1Sum = normalizedTaxes
                        .filter(t => (t.taxMainCode || '').toString().toUpperCase() === 'T1')
                        .reduce((s, t) => s + (Number(t.taxValue || 0)), 0);
                    const t2Sum = normalizedTaxes
                        .filter(t => (t.taxMainCode || '').toString().toUpperCase() === 'T2')
                        .reduce((s, t) => s + (Number(t.taxValue || 0)), 0);

                    return {
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
                        totalTaxes: item.totalTaxes ?? item.TotalTaxes ?? null,
                        finalPrice: item.finalPrice ?? item.FinalPrice ?? null,
                        productTaxes: normalizedTaxes,
                        t1Tax: t1Sum,
                        t2Tax: t2Sum,
                        createdAtUtc: new Date(item.createdAtUtc)
                    };
                });
            },

            handleFormSubmit: async () => {
                state.isSubmitting = true;
                await new Promise(resolve => setTimeout(resolve, 200));

                if (!validateForm()) {
                    state.isSubmitting = false;
                    return;
                }

                try {
                    const productTaxes = state.productTaxes
                        .filter(tax => tax.taxId)
                        .map(tax => ({
                            taxId: tax.taxId,
                            taxValue: tax.taxValue,
                            mainCode: tax.taxMainCode,
                            subCode: tax.taxSubCode,
                            percentage: tax.percentage
                        }));

                    let response = null;
                    if (state.id === '') {
                        response = await services.createMainData(
                            state.name,
                            state.barcode,
                            state.unitPrice,
                            state.description,
                            state.productGroupId,
                            state.unitMeasureId,
                            state.vatId,
                            state.physical,
                            StorageManager.getUserId(),
                            productTaxes
                        );
                    } else if (state.deleteMode) {
                        response = await services.deleteMainData(state.id, StorageManager.getUserId());
                    } else {
                        response = await services.updateMainData(
                            state.id,
                            state.name,
                            state.barcode,
                            state.unitPrice,
                            state.description,
                            state.productGroupId,
                            state.unitMeasureId,
                            state.vatId,
                            state.physical,
                            StorageManager.getUserId(),
                            productTaxes
                        );
                    }

                    // Check response code and show proper message
                    const isSuccess = response?.data?.code === 200 || response?.status === 200;
                    if (isSuccess) {
                        // on success refresh and close modal
                        await methods.populateMainData();
                        if (mainGrid.obj) mainGrid.obj.setProperties({ dataSource: state.mainData });
                        mainModal.hide();

                        const title = state.deleteMode ? 'Delete Successful' : (state.id === '' ? 'Add Successful' : 'Update Successful');
                        Swal.fire({
                            icon: 'success',
                            title,
                            timer: 2000,
                            showConfirmButton: false
                        });
                    } else {
                        // show server returned error message
                        const msg = response?.data?.message ?? 'Please check your data.';
                        const title = state.deleteMode ? 'Delete Failed' : (state.id === '' ? 'Add Failed' : 'Update Failed');
                        Swal.fire({
                            icon: 'error',
                            title,
                            text: msg,
                            confirmButtonText: 'Try Again'
                        });
                    }
                } catch (error) {
                    console.error('Save error', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'An Error Occurred',
                        text: error.response?.data?.message ?? error.message ?? 'Please try again.',
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
                        { field: 'id', isPrimaryKey: true, headerText: 'Id', visible: false },
                        { field: 'name', headerText: 'Name', width: 200, minWidth: 200 },
                        { field: 'number', headerText: 'Number', width: 150, minWidth: 150 },
                        { field: 'barcode', headerText: 'Barcode', width: 120, minWidth: 120 },
                        { field: 'internalCode', headerText: 'Internal Code', width: 120 },
                        { field: 'gisEgsCode', headerText: 'GIS / EGS Code', width: 120 },
                        { field: 'companyName', headerText: 'Company', width: 150 },
                        { field: 'productCompanyName', headerText: 'Company Name', width: 150 },
                        { field: 'model', headerText: 'Model', width: 150 },
                        { field: 'unitPrice', headerText: 'Unit Price', width: 100, format: 'N2' },
                        { field: 'discount', headerText: 'Discount', width: 100, format: 'N2' },
                        { field: 'priceAfterDiscount', headerText: 'Price After Disc.', width: 120, format: 'N2' },

                        { field: 't1Tax', headerText: 'VAT (T1)', width: 120, format: 'N2' },
                        { field: 't2Tax', headerText: 'Table Tax (T2) / ضريبة الجدول', width: 150, format: 'N2' },

                        { field: 'serviceFee', headerText: 'Service Fee', width: 100, format: 'N2' },
                        { field: 'additionalTax', headerText: 'Additional Tax', width: 100, format: 'N2' },
                        { field: 'additionalFee', headerText: 'Additional Fee', width: 100, format: 'N2' },

                        { field: 'totalTaxes', headerText: 'Total Taxes', width: 100, format: 'N2' },
                        { field: 'finalPrice', headerText: 'Final Price', width: 120, format: 'N2' },

                        { field: 'productGroupName', headerText: 'Product Group', width: 150 },
                        { field: 'unitMeasureName', headerText: 'Unit Measure', width: 150 },
                        { field: 'vatName', headerText: 'VAT', width: 150 },
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
                                state.unitPrice = selectedRecord.unitPrice ?? 0;
                                state.discount = selectedRecord.discount ?? 0;
                                state.priceAfterDiscount = (selectedRecord.priceAfterDiscount ?? selectedRecord.PriceAfterDiscount) != null
                                    ? (selectedRecord.priceAfterDiscount ?? selectedRecord.PriceAfterDiscount)
                                    : (calculatePriceAfterDiscount());
                                state.description = selectedRecord.description ?? '';
                                state.productGroupId = selectedRecord.productGroupId ?? '';
                                state.productCompanyId = selectedRecord.productCompanyId ?? null;
                                state.unitMeasureId = selectedRecord.unitMeasureId ?? '';
                                state.vatId = selectedRecord.vatId ?? '';
                                state.physical = selectedRecord.physical ?? false;
                                try {
                                    await methods.populateProductCompaniesByGroup(state.productGroupId);
                                } catch (e) {
                                    console.error('failed to populate product companies on edit', e);
                                }

                                try {
                                    const existingTaxes = selectedRecord.productTaxes || selectedRecord.ProductTaxes || [];
                                    state.productTaxes = (Array.isArray(existingTaxes) ? existingTaxes.map(t => ({
                                        id: t.taxId ?? t.TaxId ?? null,
                                        taxId: t.taxId ?? t.TaxId ?? null,
                                        taxMainCode: t.mainCode ?? t.MainCode ?? t.taxMainCode ?? '',
                                        taxSubCode: t.subCode ?? t.SubCode ?? t.taxSubCode ?? '',
                                        taxCategoryName: t.taxCategoryName ?? t.TaxCategoryName ?? '',
                                        description: t.description ?? t.Description ?? '',
                                        percentage: Number(t.percentage ?? t.Percentage ?? 0),
                                        taxValue: Number(t.taxValue ?? t.TaxValue ?? 0)
                                    })) : []);
                                } catch (err) {
                                    console.error('Failed to populate productTaxes on edit', err);
                                    state.productTaxes = [];
                                }

                                calculateTaxValues();

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
                state.priceAfterDiscount = calculatePriceAfterDiscount();
                calculateTaxValues();
            }
        );

        Vue.watch(
            () => state.discount,
            (newVal, oldVal) => {
                state.priceAfterDiscount = calculatePriceAfterDiscount();
                calculateTaxValues();
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

        Vue.onMounted(async () => {
            try {
                await SecurityManager.authorizePage(['Products']);
                await SecurityManager.validateToken();

                mainModal.create();

                await methods.populateMainData();
                await mainGrid.create(state.mainData);

                await methods.populateProductGroupListLookupData();
                productGroupListLookup.create();

                state.productCompanyMap = {};
                state.productGroupListLookupData.forEach(g => {
                    if (g && Array.isArray(g.companyIds) && g.companyIds.length) {
                        const names = (g.companyNames || '').split(',').map(s => s.trim()).filter(s => s.length);
                        state.productCompanyMap[g.id] = g.companyIds.map((id, idx) => ({ id, name: names[idx] ?? '' }));
                    }
                });

                await methods.populateUnitMeasureListLookupData();
                unitMeasureListLookup.create();

                // still load tax list for modal tax dropdowns
                await methods.populateTaxListData();

                nameText.create();
                numberText.create();
                barcodeText.create();
                unitPriceNumber.create();

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
            try { discountNumber.destroy?.(); } catch { }
            try { priceAfterDiscountNumber.destroy?.(); } catch { }
        });

        return {
            mainGridRef,
            mainModalRef,
            productGroupIdRef,
            unitMeasureIdRef,
            nameRef,
            numberRef,
            barcodeRef,
            unitPriceRef,
            state,
            handler,
            addTaxRow,
            removeTaxRow,
            onTaxMainCodeChange,
            onTaxSubCodeChange,
            getUniqueMainCodes,
            getSubCodesByMainCode,
            formatCurrency,
            calculateTotalTaxPercentage,
            calculateTotalTaxValue,
            calculatePriceAfterTaxes
        };
    }
};

Vue.createApp(App).mount('#app');