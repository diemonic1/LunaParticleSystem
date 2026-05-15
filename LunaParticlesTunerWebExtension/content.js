'use strict';

const TARGET_TITLE = 'Dev Environment - Unity Playworks';
const DOCUMENTATION_URL = 'https://github.com/diemonic1/Luna-Particles-Tuner';
const COMPATIBILITY_TABLE_URL = 'https://docs.google.com/spreadsheets/d/1yAGQokKCIa_cuVtBEkPlxS2xls-0DV8M09hwa01aYk0/edit?pli=1&gid=0#gid=0';

const PREVIEW_IFRAME_ID = 'preview-iframe';
const TOGGLE_BUTTON_ID = 'lps-settings-toggle';
const PANEL_ID = 'lps-settings-panel';
const SECTIONS_CONTAINER_ID = 'lps-sections-container';
const LOGS_ENABLED_STORAGE_KEY = 'lps-is-logs-enabled';

let iframe = null;
let toggleButton = null;
let sidePanel = null;
let sectionsContainer = null;
let isUiInitialized = false;
let isMessageSubscribed = false;
let isJsLogsEnabled = false;

function logInfo(message, details, force) {
    if (!isJsLogsEnabled && !force) {
        return;
    }

    if (details === undefined) {
        console.log(message);
        return;
    }

    console.log(message, details);
}

//#region Input factories
function createFloatInput(id, descriptor, value) {
    return buildTypedInputRow(
        id,
        descriptor,
        value,
        (input, initialValue) => {
            const numericValue = Number(initialValue);
            configureDecimalNumberInput(input, Number.isFinite(numericValue) ? numericValue.toString() : '0');
        },
        (input) => {
            const numericValue = Number.parseFloat(normalizeFloatString(input.value));
            return Number.isFinite(numericValue) ? numericValue : undefined;
        },
        normalizeDecimalInputString
    );
}

function createFloatWithRandomInput(id, descriptor, value) {
    const normalizedInitialValue = normalizeFloatWithRandomValue(value);

    if (normalizedInitialValue.includes('curve')) {
        const row = document.createElement('div');
        row.className = 'lps-field-row';

        const label = document.createElement('label');
        label.className = 'lps-field-label';
        label.textContent = descriptor.displayName;

        const readOnlyValue = document.createElement('div');
        readOnlyValue.className = 'lps-readonly-value';
        readOnlyValue.textContent = normalizedInitialValue;

        const hiddenInput = document.createElement('input');
        hiddenInput.type = 'hidden';
        hiddenInput.className = 'lps-field-input';
        hiddenInput.dataset.id = String(id);
        hiddenInput.dataset.property = descriptor.rawKey;
        hiddenInput.dataset.valueType = descriptor.valueType;
        hiddenInput._lpsGetValue = () => normalizedInitialValue;

        row.appendChild(label);
        row.appendChild(readOnlyValue);
        row.appendChild(hiddenInput);
        return row;
    }

    const [minRaw, maxRaw] = normalizedInitialValue.split('|');
    const minValue = minRaw ?? '0';
    const hasTwoValues = maxRaw !== undefined && maxRaw !== '';
    const maxValue = maxRaw ?? '';

    const row = document.createElement('div');
    row.className = 'lps-field-row';

    const label = document.createElement('label');
    label.className = 'lps-field-label';
    label.textContent = descriptor.displayName;

    const controls = document.createElement('div');

    if (hasTwoValues)
        controls.className = 'lps-float-random-controls';

    const minInput = document.createElement('input');
    minInput.className = 'lps-field-input lps-float-random-input';
    configureDecimalNumberInput(minInput, minValue);

    const collectorInput = document.createElement('input');
    collectorInput.type = 'hidden';
    collectorInput.className = 'lps-field-input';
    collectorInput.dataset.id = String(id);
    collectorInput.dataset.property = descriptor.rawKey;
    collectorInput.dataset.valueType = descriptor.valueType;

    controls.appendChild(minInput);

    if (hasTwoValues) {
        const separator = document.createElement('span');
        separator.className = 'lps-float-random-separator';
        separator.textContent = '|';

        const maxInput = document.createElement('input');
        maxInput.className = 'lps-field-input lps-float-random-input';
        configureDecimalNumberInput(maxInput, maxValue);
        maxInput.placeholder = 'max';

        collectorInput._lpsGetValue = () => {
            const minNumber = Number.parseFloat(normalizeFloatString(minInput.value));
            if (!Number.isFinite(minNumber)) {
                return undefined;
            }

            const maxText = normalizeFloatString(maxInput.value).trim();
            if (!maxText) {
                return minNumber.toString();
            }

            const maxNumber = Number.parseFloat(maxText);
            if (!Number.isFinite(maxNumber)) {
                return undefined;
            }

            return `${minNumber.toString()}|${maxNumber.toString()}`;
        };

        const onInputChanged = () => {
            normalizeInputValueWithCaret(minInput, normalizeDecimalInputString);
            normalizeInputValueWithCaret(maxInput, normalizeDecimalInputString);
            handleFieldChanged(collectorInput);
        };

        minInput.addEventListener('input', onInputChanged);
        maxInput.addEventListener('input', onInputChanged);

        controls.appendChild(separator);
        controls.appendChild(maxInput);
    } else {
        collectorInput._lpsGetValue = () => {
            const minNumber = Number.parseFloat(normalizeFloatString(minInput.value));
            if (!Number.isFinite(minNumber)) {
                return undefined;
            }

            return minNumber.toString();
        };

        const onInputChanged = () => {
            normalizeInputValueWithCaret(minInput, normalizeDecimalInputString);
            handleFieldChanged(collectorInput);
        };

        minInput.addEventListener('input', onInputChanged);
    }

    row.appendChild(label);
    row.appendChild(controls);
    row.appendChild(collectorInput);

    return row;
}

function createIntInput(id, descriptor, value) {
    return buildTypedInputRow(
        id,
        descriptor,
        value,
        (input, initialValue) => {
            const numericValue = Number(initialValue);
            input.type = 'number';
            input.step = '1';
            input.inputMode = 'numeric';
            input.value = Number.isFinite(numericValue) ? Math.trunc(numericValue).toString() : '0';
        },
        (input) => {
            const numericValue = Number.parseInt(normalizeIntString(input.value), 10);
            return Number.isInteger(numericValue) ? numericValue : undefined;
        },
        normalizeIntString
    );
}

function createBoolInput(id, descriptor, value) {
    const row = document.createElement('div');
    row.className = 'lps-field-row';

    const label = document.createElement('label');
    label.className = 'lps-field-label lps-bool-label';
    label.textContent = descriptor.displayName;

    const input = document.createElement('input');
    input.className = 'lps-field-input lps-bool-input';
    input.type = 'checkbox';
    input.dataset.id = String(id);
    input.dataset.property = descriptor.rawKey;
    input.dataset.valueType = descriptor.valueType;
    input.id = buildInputId(id, descriptor.rawKey);
    label.htmlFor = input.id;
    input.checked = value === true || value === 'true' || value === 1;

    input._lpsGetValue = () => input.checked;

    input.addEventListener('change', () => handleFieldChanged(input));

    row.appendChild(label);
    row.appendChild(input);
    return row;
}

function createColorInput(id, descriptor, value) {
    const normalizedInitialColor = normalizeColorString(value);
    const row = buildTypedInputRow(
        id,
        descriptor,
        normalizedInitialColor,
        (input, initialValue) => {
            input.value = normalizeColorString(initialValue);
            input.classList.add('lps-color-text-input');
            input.setAttribute('data-jscolor', '{"alphaChannel":true,"format":"hexa"}');
        },
        (input) => tryNormalizeColorString(input.value),
        undefined
    );

    const input = row.querySelector('.lps-field-input');
    new jscolor(input);
    input.addEventListener('change', () => {
        const normalized = tryNormalizeColorString(input.value) || normalizedInitialColor;
        input.value = normalized;
        handleFieldChanged(input);
    });

    return row;
}

function createSelectOneInput(id, descriptor, value) {
    const rawValue = String(value ?? '');
    const [currentRaw, optionsRaw] = rawValue.split('|');
    const currentValue = String(currentRaw ?? '').trim();
    const options = String(optionsRaw ?? '')
        .split(',')
        .map((item) => item.trim())
        .filter((item) => item.length > 0);

    if (currentValue && !options.includes(currentValue)) {
        options.unshift(currentValue);
    }

    if (options.length === 0) {
        options.push(currentValue || '');
    }

    const row = document.createElement('div');
    row.className = 'lps-field-row';

    const label = document.createElement('label');
    label.className = 'lps-field-label';
    label.textContent = descriptor.displayName;

    const select = document.createElement('select');
    select.className = 'lps-field-input lps-select-input';
    select.dataset.id = String(id);
    select.dataset.property = descriptor.rawKey;
    select.dataset.valueType = descriptor.valueType;
    select.id = buildInputId(id, descriptor.rawKey);
    label.htmlFor = select.id;

    options.forEach((optionValue) => {
        const option = document.createElement('option');
        option.value = optionValue;
        option.textContent = optionValue;
        select.appendChild(option);
    });

    select.value = currentValue || options[0];
    select._lpsGetValue = () => select.value;
    select.addEventListener('change', () => handleFieldChanged(select));

    row.appendChild(label);
    row.appendChild(select);
    return row;
}

function createVector3Input(id, descriptor, value) {
    const rawValue = String(value ?? '').trim();
    const parts = rawValue.split('|');
    const xValue = parts[0] ?? '0';
    const yValue = parts[1] ?? '0';
    const zValue = parts[2] ?? '0';

    const row = document.createElement('div');
    row.className = 'lps-field-row lps-field-row-vector3';

    const label = document.createElement('label');
    label.className = 'lps-field-label';
    label.textContent = descriptor.displayName;

    const controls = document.createElement('div');
    controls.className = 'lps-vector3-controls';

    const xInput = document.createElement('input');
    xInput.className = 'lps-field-input lps-vector3-input';
    configureDecimalNumberInput(xInput, xValue);

    const yInput = document.createElement('input');
    yInput.className = 'lps-field-input lps-vector3-input';
    configureDecimalNumberInput(yInput, yValue);

    const zInput = document.createElement('input');
    zInput.className = 'lps-field-input lps-vector3-input';
    configureDecimalNumberInput(zInput, zValue);

    const appendVectorComponent = (componentName, input) => {
        const component = document.createElement('div');
        component.className = 'lps-vector3-component';

        const componentLabel = document.createElement('span');
        componentLabel.className = 'lps-vector3-component-label';
        componentLabel.textContent = componentName;

        component.appendChild(componentLabel);
        component.appendChild(input);
        controls.appendChild(component);
    };

    const collectorInput = document.createElement('input');
    collectorInput.type = 'hidden';
    collectorInput.className = 'lps-field-input';
    collectorInput.dataset.id = String(id);
    collectorInput.dataset.property = descriptor.rawKey;
    collectorInput.dataset.valueType = descriptor.valueType;

    collectorInput._lpsGetValue = () => {
        const x = Number.parseFloat(normalizeFloatString(xInput.value));
        const y = Number.parseFloat(normalizeFloatString(yInput.value));
        const z = Number.parseFloat(normalizeFloatString(zInput.value));

        if (!Number.isFinite(x) || !Number.isFinite(y) || !Number.isFinite(z)) {
            return undefined;
        }

        return `${x.toString()}|${y.toString()}|${z.toString()}`;
    };

    const onInputChanged = () => {
        normalizeInputValueWithCaret(xInput, normalizeDecimalInputString);
        normalizeInputValueWithCaret(yInput, normalizeDecimalInputString);
        normalizeInputValueWithCaret(zInput, normalizeDecimalInputString);
        handleFieldChanged(collectorInput);
    };

    xInput.addEventListener('input', onInputChanged);
    yInput.addEventListener('input', onInputChanged);
    zInput.addEventListener('input', onInputChanged);

    appendVectorComponent('X', xInput);
    appendVectorComponent('Y', yInput);
    appendVectorComponent('Z', zInput);

    row.appendChild(label);
    row.appendChild(controls);
    row.appendChild(collectorInput);

    return row;
}
//#endregion

function createInputForSetting(id, rawKey, value) {
    const descriptor = parseSettingDescriptor(rawKey);

    switch (descriptor.valueType) {
        case 'int':
            return createIntInput(id, descriptor, value);
        case 'float_with_random':
            return createFloatWithRandomInput(id, descriptor, value);
        case 'bool':
            return createBoolInput(id, descriptor, value);
        case 'color':
            return createColorInput(id, descriptor, value);
        case 'select_one':
            return createSelectOneInput(id, descriptor, value);
        case 'vector_3':
            return createVector3Input(id, descriptor, value);
        case 'float':
            return createFloatInput(id, descriptor, value);
        default:
            throw new Error(`[Luna Particles Tuner JS] Unsupported value type: ${descriptor.valueType} for key: ${rawKey}`);
    }
}

// other

//#region Extension

function sendDataToLuna(payloadObject) {
    try {
        if (!iframe || !iframe.contentWindow) {
            throw new Error('preview iframe is not available.');
        }

        iframe.contentWindow.postMessage({
            type: 'LPS_DATA_PS_SETTINGS',
            payload: payloadObject
        }, '*');

        logInfo('[Luna Particles Tuner JS] Sent data to Unity.', { payloadObject });
    } catch (error) {
        console.error('[Luna Particles Tuner JS] Failed to send speed to Unity.', { payloadObject, error });
    }
}

function sendSingleFieldUpdate(sectionId, propertyKey, value) {
    if (!sectionId || !propertyKey || value === undefined) {
        logInfo('[Luna Particles Tuner JS] Skip sending: missing id, key or value.', { sectionId, propertyKey, value });
        return;
    }

    sendDataToLuna({
        data: [{
            id: sectionId,
            settings: { [propertyKey]: value }
        }]
    });
}

function handleFieldChanged(collectorInput) {
    const sectionId = collectorInput.dataset.id;
    const propertyKey = collectorInput.dataset.property;
    const value = getInputValue(collectorInput);
    sendSingleFieldUpdate(sectionId, propertyKey, value);
}

function collectSectionSettings(section) {
    const sectionId = section.dataset.id || '';
    const settings = {};
    const inputs = section.querySelectorAll('.lps-field-input[data-property]');

    inputs.forEach((input) => {
        const propertyName = input.dataset.property;
        const settingValue = getInputValue(input);

        if (!propertyName || settingValue === undefined) {
            return;
        }

        settings[propertyName] = settingValue;
    });

    return {
        id: sectionId,
        settings
    };
}

function copySectionToClipboard(section) {
    const sectionData = collectSectionSettings(section);
    const payload = {
        data: [sectionData]
    };

    const jsonString = JSON.stringify(payload);

    try {
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(jsonString).then(() => {
                logInfo('[Luna Particles Tuner JS] Section settings copied to clipboard.', { sectionId: sectionData.id }, true);
            }).catch((error) => {
                console.error('[Luna Particles Tuner JS] Failed to copy to clipboard.', { error });
            });
        } else {
            const textarea = document.createElement('textarea');
            textarea.value = jsonString;
            textarea.style.position = 'fixed';
            textarea.style.opacity = '0';
            document.body.appendChild(textarea);
            textarea.select();
            document.execCommand('copy');
            document.body.removeChild(textarea);
            logInfo('[Luna Particles Tuner JS] Section settings copied to clipboard (fallback).', { sectionId: sectionData.id }, true);
        }
    } catch (error) {
        console.error('[Luna Particles Tuner JS] Failed to copy section settings to clipboard.', { error });
    }
}

function saveIsLogsEnabledToStorage() {
    try {
        localStorage.setItem(LOGS_ENABLED_STORAGE_KEY, isJsLogsEnabled ? '1' : '0');
    } catch (error) {
        logInfo('[Luna Particles Tuner JS] Failed to persist logs state.', { error }, true);
    }
}

function restoreIsLogsEnabledFromStorage() {
    try {
        const savedValue = localStorage.getItem(LOGS_ENABLED_STORAGE_KEY);
        if (savedValue === null) {
            return;
        }

        isJsLogsEnabled = savedValue === '1' || savedValue === 'true';
    } catch (error) {
        logInfo('[Luna Particles Tuner JS] Failed to restore logs state.', { error }, true);
    }
}

function openExternalPage(url) {
    window.open(url, '_blank', 'noopener,noreferrer');
}

function ensureUiRoot() {
    if (isUiInitialized) {
        return;
    }

    toggleButton = document.createElement('button');
    toggleButton.id = TOGGLE_BUTTON_ID;
    toggleButton.type = 'button';
    toggleButton.textContent = 'Luna Particles Tuner';
    toggleButton.addEventListener('click', () => setPanelVisibility(true));

    sidePanel = document.createElement('aside');
    sidePanel.id = PANEL_ID;

    const panelHeader = document.createElement('div');
    panelHeader.className = 'lps-panel-header';

    const panelTitle = document.createElement('div');
    panelTitle.textContent = 'Luna Particles Tuner';

    const closeButton = document.createElement('button');
    closeButton.type = 'button';
    closeButton.className = 'lps-panel-close';
    closeButton.textContent = '×';
    closeButton.setAttribute('aria-label', 'Close Luna Particles Tuner');
    closeButton.addEventListener('click', () => setPanelVisibility(false));

    sectionsContainer = document.createElement('div');
    sectionsContainer.id = SECTIONS_CONTAINER_ID;

    panelHeader.appendChild(panelTitle);
    panelHeader.appendChild(closeButton);
    sidePanel.appendChild(panelHeader);

    const linksContainer = document.createElement('div');
    linksContainer.className = 'lps-links-container';

    const docsButton = document.createElement('button');
    docsButton.type = 'button';
    docsButton.className = 'lps-link-button';
    docsButton.textContent = 'Open documentation';
    docsButton.addEventListener('click', () => openExternalPage(DOCUMENTATION_URL));

    const compatibilityButton = document.createElement('button');
    compatibilityButton.type = 'button';
    compatibilityButton.className = 'lps-link-button';
    compatibilityButton.textContent = 'Open compatibility table';
    compatibilityButton.addEventListener('click', () => openExternalPage(COMPATIBILITY_TABLE_URL));

    linksContainer.appendChild(docsButton);
    linksContainer.appendChild(compatibilityButton);

    const logsContainer = document.createElement('div');
    logsContainer.className = 'lps-timescale-container lps-logs-container';

    const logsLabel = document.createElement('label');
    logsLabel.className = 'lps-logs-label lps-field-label';
    logsLabel.textContent = 'Enable Logs';

    const logsCheckbox = document.createElement('input');
    logsCheckbox.type = 'checkbox';
    logsCheckbox.className = 'lps-logs-checkbox lps-bool-input';
    logsCheckbox.checked = isJsLogsEnabled;
    logsLabel.htmlFor = 'lps-enable-logs-checkbox';
    logsCheckbox.id = 'lps-enable-logs-checkbox';
    logsCheckbox.addEventListener('change', () => {
        isJsLogsEnabled = logsCheckbox.checked;
        saveIsLogsEnabledToStorage();
        sendIsLogsEnabledToLuna();

        logInfo(
            isJsLogsEnabled
                ? '[Luna Particles Tuner JS] Logs enabled'
                : '[Luna Particles Tuner JS] Logs disabled',
            undefined,
            true
        );
    });

    logsContainer.appendChild(logsLabel);
    logsContainer.appendChild(logsCheckbox);

    const timeScaleContainer = document.createElement('div');
    timeScaleContainer.className = 'lps-timescale-container';

    const timeScaleLabel = document.createElement('label');
    timeScaleLabel.className = 'lps-timescale-label';
    timeScaleLabel.textContent = 'TimeScale: 1.00';

    const timeScaleSlider = document.createElement('input');
    timeScaleSlider.type = 'range';
    timeScaleSlider.className = 'lps-timescale-slider';
    timeScaleSlider.min = '0';
    timeScaleSlider.max = '1';
    timeScaleSlider.step = '0.01';
    timeScaleSlider.value = '1';
    timeScaleSlider.addEventListener('input', () => {
        const value = parseFloat(timeScaleSlider.value);
        timeScaleLabel.textContent = `TimeScale: ${value.toFixed(2)}`;
        iframe.contentWindow.postMessage({
            type: 'LPS_DATA_TimeScale',
            payload: String(value)
        }, '*');
    });

    timeScaleContainer.appendChild(timeScaleLabel);
    timeScaleContainer.appendChild(timeScaleSlider);
    sidePanel.appendChild(linksContainer);
    sidePanel.appendChild(logsContainer);
    sidePanel.appendChild(timeScaleContainer);
    sidePanel.appendChild(sectionsContainer);

    document.body.appendChild(toggleButton);
    document.body.appendChild(sidePanel);

    setPanelVisibility(false);
    isUiInitialized = true;
}

function setPanelVisibility(isVisible) {
    if (!sidePanel || !toggleButton) {
        return;
    }

    sidePanel.classList.toggle('lps-open', isVisible);
    toggleButton.style.display = isVisible ? 'none' : 'block';
}

function parseIncomingPayload(payload) {
    if (payload == null) {
        throw new Error('Payload is empty.');
    }

    if (typeof payload === 'object') {
        return payload;
    }

    if (typeof payload !== 'string') {
        throw new Error(`Unsupported payload type: ${typeof payload}`);
    }

    const normalizedPayload = normalizePayloadString(payload);

    try {
        return JSON.parse(normalizedPayload);
    } catch (jsonError) {
        throw new Error(`Failed to parse payload. JSON error: ${jsonError.message}. Payload: ${normalizedPayload}`);
    }
}

function normalizePayloadString(payload) {
    const trimmedPayload = payload.trim();
    if (!trimmedPayload) {
        throw new Error('Payload string is empty.');
    }

    return trimmedPayload
        .replace(/([{,]\s*)([A-Za-z_$][\w$]*)(\s*:)/g, '$1"$2"$3')
        .replace(/:\s*'([^']*)'/g, ': "$1"');
}

function parseSettingDescriptor(rawKey) {
    const safeRawKey = String(rawKey ?? '');
    const firstSep = safeRawKey.indexOf('|');

    if (firstSep <= 0) {
        return { rawKey: safeRawKey, displayName: safeRawKey, valueType: 'float', section: 'main' };
    }

    const displayName = safeRawKey.substring(0, firstSep);
    const rest = safeRawKey.substring(firstSep + 1);
    const secondSep = rest.indexOf('|');

    if (secondSep < 0) {
        return { rawKey: safeRawKey, displayName, valueType: rest.toLowerCase(), section: 'main' };
    }

    return {
        rawKey: safeRawKey,
        displayName,
        valueType: rest.substring(0, secondSep).toLowerCase(),
        section: rest.substring(secondSep + 1).toLowerCase()
    };
}

function buildInputId(id, rawKey) {
    const normalizedKey = String(rawKey).replace(/[^a-zA-Z0-9_-]/g, '-');
    return `lps-input-${String(id)}-${normalizedKey}`;
}

function normalizeFloatString(value) {
    return normalizeDecimalInputString(value);
}

function normalizeDecimalInputString(value) {
    const source = String(value ?? '');
    if (!source) {
        return '';
    }

    let result = '';
    let hasMinus = false;
    let hasDot = false;

    for (let i = 0; i < source.length; i += 1) {
        const char = source[i];

        if (char >= '0' && char <= '9') {
            result += char;
            continue;
        }

        if (char === '-' && !hasMinus && result.length === 0) {
            hasMinus = true;
            result += char;
            continue;
        }

        if ((char === '.' || char === ',') && !hasDot) {
            hasDot = true;
            result += '.';
        }
    }

    return result;
}

function configureDecimalNumberInput(input, initialValue) {
    input.type = 'number';
    input.inputMode = 'decimal';
    input.step = '0.1';
    input.value = normalizeDecimalInputString(initialValue);

    input.addEventListener('keydown', (event) => {
        if (event.key !== '.' && event.key !== ',') {
            return;
        }

        event.preventDefault();
        insertNormalizedTextAtCaret(input, '.');
    });
}

function insertNormalizedTextAtCaret(input, insertedText) {
    const currentValue = String(input.value ?? '');
    const selectionStart = typeof input.selectionStart === 'number' ? input.selectionStart : currentValue.length;
    const selectionEnd = typeof input.selectionEnd === 'number' ? input.selectionEnd : selectionStart;
    const nextRawValue = `${currentValue.slice(0, selectionStart)}${insertedText}${currentValue.slice(selectionEnd)}`;
    const normalizedNextValue = normalizeDecimalInputString(nextRawValue);
    const normalizedBeforeCaret = normalizeDecimalInputString(`${currentValue.slice(0, selectionStart)}${insertedText}`);

    input.value = normalizedNextValue;

    if (typeof input.setSelectionRange === 'function') {
        try {
            input.setSelectionRange(normalizedBeforeCaret.length, normalizedBeforeCaret.length);
        } catch (error) {
            // Selection APIs are limited for some number input implementations.
        }
    }
}

function normalizeInputValueWithCaret(input, normalizeInputValue) {
    const currentValue = String(input.value ?? '');
    const selectionStart = typeof input.selectionStart === 'number' ? input.selectionStart : currentValue.length;
    const selectionEnd = typeof input.selectionEnd === 'number' ? input.selectionEnd : selectionStart;
    const normalizedValue = normalizeInputValue(currentValue);

    if (normalizedValue === currentValue) {
        return;
    }

    const normalizedBeforeStart = normalizeInputValue(currentValue.slice(0, selectionStart));
    const normalizedBeforeEnd = normalizeInputValue(currentValue.slice(0, selectionEnd));

    input.value = normalizedValue;

    if (typeof input.setSelectionRange === 'function') {
        try {
            input.setSelectionRange(normalizedBeforeStart.length, normalizedBeforeEnd.length);
        } catch (error) {
            // Selection APIs are limited for some number input implementations.
        }
    }
}

function normalizeIntString(value) {
    const normalizedValue = normalizeFloatString(value).trim();

    if (normalizedValue === '-') {
        return '-';
    }

    const match = normalizedValue.match(/^-?\d+/);
    return match ? match[0] : '';
}

function normalizeFloatWithRandomValue(value) {
    const rawValue = String(value ?? '').trim();
    if (!rawValue) {
        return '0';
    }

    if (rawValue.includes('curve')) {
        return rawValue;
    }

    const split = rawValue.split('|');
    if (split.length === 1) {
        const singleValue = Number.parseFloat(normalizeFloatString(split[0]));
        return Number.isFinite(singleValue) ? singleValue.toString() : '0';
    }

    if (split.length === 2) {
        const minValue = Number.parseFloat(normalizeFloatString(split[0]));
        const maxValue = Number.parseFloat(normalizeFloatString(split[1]));
        if (Number.isFinite(minValue) && Number.isFinite(maxValue)) {
            return `${minValue.toString()}|${maxValue.toString()}`;
        }
    }

    return '0';
}

function normalizeColorString(value) {
    const normalizedValue = String(value ?? '').trim();
    if (!normalizedValue) {
        return '#FFFFFFFF';
    }

    const withHash = normalizedValue.startsWith('#') ? normalizedValue : `#${normalizedValue}`;
    const hex = withHash.substring(1);

    if (/^[0-9a-fA-F]{3}$/.test(hex) || /^[0-9a-fA-F]{4}$/.test(hex) || /^[0-9a-fA-F]{6}$/.test(hex) || /^[0-9a-fA-F]{8}$/.test(hex)) {
        return withHash.toUpperCase();
    }

    return '#FFFFFFFF';
}

function tryNormalizeColorString(value) {
    const normalizedValue = String(value ?? '').trim();
    if (!normalizedValue) {
        return undefined;
    }

    const withHash = normalizedValue.startsWith('#') ? normalizedValue : `#${normalizedValue}`;
    const hex = withHash.substring(1);

    if (/^[0-9a-fA-F]{3}$/.test(hex) || /^[0-9a-fA-F]{4}$/.test(hex) || /^[0-9a-fA-F]{6}$/.test(hex) || /^[0-9a-fA-F]{8}$/.test(hex)) {
        return withHash.toUpperCase();
    }

    return undefined;
}

function buildTypedInputRow(id, descriptor, value, configureInput, readValue, normalizeInputValue) {
    const row = document.createElement('div');
    row.className = 'lps-field-row';

    const label = document.createElement('label');
    label.className = 'lps-field-label';
    label.textContent = descriptor.displayName;

    const input = document.createElement('input');
    input.className = 'lps-field-input';
    input.dataset.id = String(id);
    input.dataset.property = descriptor.rawKey;
    input.dataset.valueType = descriptor.valueType;
    input.id = buildInputId(id, descriptor.rawKey);
    label.htmlFor = input.id;

    configureInput(input, value);
    input._lpsGetValue = () => readValue(input);

    input.addEventListener('input', () => {
        if (typeof normalizeInputValue === 'function') {
            normalizeInputValueWithCaret(input, normalizeInputValue);
        }

        handleFieldChanged(input);
    });

    row.appendChild(label);
    row.appendChild(input);
    return row;
}

function getInputValue(input) {
    if (typeof input._lpsGetValue === 'function') {
        return input._lpsGetValue();
    }

    const numericValue = Number.parseFloat(normalizeFloatString(input.value));
    return Number.isFinite(numericValue) ? numericValue : undefined;
}

function createSectionElement(id, name) {
    const section = document.createElement('section');
    section.className = 'lps-section lps-collapsed';
    section.dataset.id = String(id);

    const header = document.createElement('button');
    header.type = 'button';
    header.className = 'lps-section-header';

    const title = document.createElement('span');
    title.className = 'lps-section-title';
    title.textContent = `${String(id)}: ${name}`;

    const copyButton = document.createElement('button');
    copyButton.type = 'button';
    copyButton.className = 'lps-section-copy-button';
    copyButton.textContent = '📋';
    copyButton.setAttribute('title', 'Copy section settings to clipboard');
    copyButton.addEventListener('click', (event) => {
        event.stopPropagation();
        copySectionToClipboard(section);
    });

    const arrow = document.createElement('span');
    arrow.className = 'lps-section-arrow';
    arrow.textContent = '▾';

    header.appendChild(title);
    header.appendChild(copyButton);
    header.appendChild(arrow);
    header.addEventListener('click', () => {
        section.classList.toggle('lps-collapsed');
    });

    const body = document.createElement('div');
    body.className = 'lps-section-body';

    section.appendChild(header);
    section.appendChild(body);
    return section;
}

function createSubsectionElement(sectionName) {
    const subsection = document.createElement('div');
    subsection.className = 'lps-subsection lps-collapsed';

    const header = document.createElement('button');
    header.type = 'button';
    header.className = 'lps-subsection-header';

    const title = document.createElement('span');
    title.className = 'lps-subsection-title';
    title.textContent = sectionName;

    const arrow = document.createElement('span');
    arrow.className = 'lps-section-arrow';
    arrow.textContent = '▾';

    header.appendChild(title);
    header.appendChild(arrow);
    header.addEventListener('click', () => {
        subsection.classList.toggle('lps-collapsed');
    });

    const body = document.createElement('div');
    body.className = 'lps-subsection-body';

    subsection.appendChild(header);
    subsection.appendChild(body);
    return subsection;
}

function upsertSection(payload) {
    ensureUiRoot();

    const id = String(payload.id ?? '');
    const name = payload.name || 'Unnamed Particle';
    const settings = payload.settings && typeof payload.settings === 'object' ? payload.settings : {};

    if (!id) {
        throw new Error('Payload id is missing.');
    }

    let section = Array.from(sectionsContainer.querySelectorAll('.lps-section'))
        .find((item) => item.dataset.id === id);
    if (!section) {
        section = createSectionElement(id, name);
        sectionsContainer.appendChild(section);
    }

    const title = section.querySelector('.lps-section-title');
    if (title) {
        title.textContent = `${id}: ${name}`;
    }

    const body = section.querySelector('.lps-section-body');
    body.innerHTML = '';

    const bySection = {};
    Object.entries(settings).forEach(([rawKey, value]) => {
        const descriptor = parseSettingDescriptor(rawKey);
        const sec = descriptor.section || 'main';
        if (!bySection[sec]) bySection[sec] = [];
        bySection[sec].push({ rawKey, value });
    });

    (bySection['main'] || []).forEach(({ rawKey, value }) => {
        body.appendChild(createInputForSetting(id, rawKey, value));
    });

    Object.entries(bySection).forEach(([sectionName, fields]) => {
        if (sectionName === 'main') return;

        const subsection = createSubsectionElement(sectionName);
        const subsectionBody = subsection.querySelector('.lps-subsection-body');
        fields.forEach(({ rawKey, value }) => {
            subsectionBody.appendChild(createInputForSetting(id, rawKey, value));
        });
        body.appendChild(subsection);
    });
}


function sendIsLogsEnabledToLuna() {
    if (!iframe || !iframe.contentWindow) {
        return;
    }

    iframe.contentWindow.postMessage({
        type: 'LPS_DATA_EnableLogs',
        payload: String(isJsLogsEnabled)
    }, '*');
}

function setupUI(rawPayload) {
    const payload = parseIncomingPayload(rawPayload);

    logInfo('[Luna Particles Tuner JS] Payload received.', payload);
    upsertSection(payload);
}

function setupIframe() {
    iframe = document.getElementById(PREVIEW_IFRAME_ID);
    if (!iframe || !iframe.contentWindow) {
        return false;
    }

    restoreIsLogsEnabledFromStorage();

    if (isMessageSubscribed) {
        return true;
    }

    window.addEventListener('message', (e) => {
        if (!e.data || e.data.type !== 'LPS_DATA_StartSetup') {
            return;
        }

        try {
            sendIsLogsEnabledToLuna();
            setupUI(e.data.payload);
        } catch (error) {
            console.error('[Luna Particles Tuner JS] Failed to setup UI.', error);
        }
    });

    isMessageSubscribed = true;

    return true;
}

function init() {
    if (document.title !== TARGET_TITLE) return;

    if (setupIframe()) return;

    const observer = new MutationObserver(() => {
        if (setupIframe()) {
            observer.disconnect();
        }
    });

    observer.observe(document.body, { childList: true, subtree: true });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
} else {
    init();
}

//#endregion
