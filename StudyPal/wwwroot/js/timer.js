let timerInterval;
let isRunning = false;
let isPaused = false;
let totalSeconds = 0;
let originalTotalSeconds = 0;
let currentSubject = '';
let sessionStartTime = null;
let isFloatingMinimized = false;

// Main timer UI

const studyForm = document.getElementById('studyForm');
const timerSection = document.getElementById('timerSection');
const timerDisplay = document.getElementById('timerDisplay');
const studySubject = document.getElementById('studySubject');
const sessionDuration = document.getElementById('sessionDuration');
const progressBar = document.getElementById('progressBar');
const pauseBtn = document.getElementById('pauseBtn');
const resumeBtn = document.getElementById('resumeBtn');
const stopBtn = document.getElementById('stopBtn');
// Floating timer UI
const floatingTimer = document.getElementById('floatingTimer');
const floatingDisplay = document.getElementById('floatingDisplay');
const floatingSubject = document.getElementById('floatingSubject');
const floatingPauseBtn = document.getElementById('floatingPauseBtn');
const floatingResumeBtn = document.getElementById('floatingResumeBtn');
const floatingStopBtn = document.getElementById('floatingStopBtn');
// Modal
const sessionCompleteModal = document.getElementById('sessionCompleteModal');
const completedSubject = document.getElementById('completedSubject');
const completedDuration = document.getElementById('completedDuration');

// Load state on page load
window.addEventListener('load', function () {
    if (typeof isUserLoggedIn !== 'undefined' && isUserLoggedIn) {
        loadTimerState();
    }
});

// Save state
function saveTimerState() {
    const timerState = {
        totalSeconds,
        originalTotalSeconds,
        isRunning,
        isPaused,
        currentSubject,
        timestamp: Date.now(),
        sessionStartTime: sessionStartTime ? sessionStartTime.toISOString() : null,
        sessionCompleted: totalSeconds <= 0
    };
    localStorage.setItem('timerState', JSON.stringify(timerState));
}

// Load state
function loadTimerState() {
    const saved = localStorage.getItem('timerState');
    const alertShownKey = 'pausedSessionAlertShown';

    if (!saved) {
        localStorage.removeItem(alertShownKey);
        return;
    }

    const state = JSON.parse(saved);
    const timePassed = Math.floor((Date.now() - state.timestamp) / 1000);
    const remainingSeconds = Math.max(0, state.totalSeconds - (state.isRunning ? timePassed : 0));

    totalSeconds = remainingSeconds;
    originalTotalSeconds = state.originalTotalSeconds;
    currentSubject = state.currentSubject;
    sessionStartTime = state.sessionStartTime ? new Date(state.sessionStartTime) : null;

    if (localStorage.getItem('loggedOutDuringSession') === 'true') {
        isRunning = false;
        isPaused = true;
        localStorage.removeItem('loggedOutDuringSession');
    }

    if (state.isRunning && remainingSeconds > 0) {
        isRunning = true;
        isPaused = false;
    } else if (state.isPaused && remainingSeconds > 0) {
        isRunning = false;
        isPaused = true;
    } else {
        isRunning = false;
        isPaused = false;
    }

    // Only show session complete ONCE
    if (remainingSeconds <= 0 && state.sessionCompleted) {
        localStorage.removeItem('timerState');
        localStorage.removeItem(alertShownKey);

        if (!localStorage.getItem('sessionCompleteShown')) {
            showSessionComplete();
        }
        return;
    }

    // Update UI elements
    if (studySubject) studySubject.textContent = currentSubject;
    if (floatingSubject) floatingSubject.textContent = currentSubject;
    if (sessionDuration) sessionDuration.textContent = formatDuration(originalTotalSeconds);
    if (studyForm) studyForm.style.display = 'none';
    if (timerSection) timerSection.classList.add('show');
    showFloatingTimer();
    updateDisplay();
    updateFloatingControls();

    if (isRunning) {
        startTimer();
    }

    if (isPaused && !localStorage.getItem(alertShownKey) && typeof isUserLoggedIn !== 'undefined' && isUserLoggedIn) {
        Swal.fire({
            title: `Welcome back!`,
            html: `You still have <b>${formatTime(totalSeconds)}</b> left on <b>${currentSubject}</b>.<br><br>Click "Resume" when you're ready.`,
            icon: 'info',
            confirmButtonText: 'Got it!',
            confirmButtonColor: '#3085d6'
        });
        localStorage.setItem(alertShownKey, 'true');
    }
}

// Helpers
function formatTime(seconds) {
    const h = String(Math.floor(seconds / 3600)).padStart(2, '0');
    const m = String(Math.floor((seconds % 3600) / 60)).padStart(2, '0');
    const s = String(seconds % 60).padStart(2, '0');
    return `${h}:${m}:${s}`;
}

function formatDuration(seconds) {
    const h = Math.floor(seconds / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    const s = seconds % 60;
    if (h > 0) return `${h}h ${m}m`;
    if (m > 0) return `${m}m ${s}s`;
    return `${s}s`;
}

function finishNow() {
    // Save session and clear everything
    saveSessionToDatabase();
    if (sessionCompleteModal) {
        sessionCompleteModal.classList.remove('show');
        sessionCompleteModal.style.display = 'none';
    }
    clearInterval(timerInterval);
    isRunning = false;
    isPaused = false;
    totalSeconds = 0;
    originalTotalSeconds = 0;
    currentSubject = '';
    sessionStartTime = null;
    localStorage.removeItem('timerState');
    localStorage.removeItem('sessionCompleteShown');
    localStorage.removeItem('pausedSessionAlertShown');

    if (timerDisplay) timerDisplay.classList.remove('finished');
    if (progressBar) progressBar.style.width = '0%';
    if (studyForm) studyForm.style.display = 'block';
    if (timerSection) timerSection.classList.remove('show');
    hideFloatingTimer();
    if (pauseBtn) pauseBtn.style.display = 'inline-block';
    if (resumeBtn) resumeBtn.style.display = 'none';
    if (floatingPauseBtn) floatingPauseBtn.style.display = 'inline-block';
    if (floatingResumeBtn) floatingResumeBtn.style.display = 'none';
    localStorage.removeItem('sessionCompleteShown');
    localStorage.removeItem('timerState'); // If appropriate for a finished session

}

function updateDisplay() {
    const timeStr = formatTime(totalSeconds);
    if (timerDisplay) timerDisplay.textContent = timeStr;
    if (floatingDisplay) floatingDisplay.textContent = timeStr;
    if (progressBar) progressBar.style.width = ((originalTotalSeconds - totalSeconds) / originalTotalSeconds * 100) + '%';
    saveTimerState();
    // Always update floating display as well
    if (floatingSubject) floatingSubject.textContent = currentSubject;
}

function updateFloatingControls() {
    if (floatingPauseBtn && floatingResumeBtn) {
        if (isRunning) {
            floatingPauseBtn.style.display = 'inline-block';
            floatingResumeBtn.style.display = 'none';
        } else if (isPaused) {
            floatingPauseBtn.style.display = 'none';
            floatingResumeBtn.style.display = 'inline-block';
        }
    }
    if (pauseBtn && resumeBtn) {
        if (isRunning) {
            pauseBtn.style.display = 'inline-block';
            resumeBtn.style.display = 'none';
        } else if (isPaused) {
            pauseBtn.style.display = 'none';
            resumeBtn.style.display = 'inline-block';
        }
    }
}

function startTimer() {
    if (!sessionStartTime) sessionStartTime = new Date();
    clearInterval(timerInterval);
    isRunning = true;
    isPaused = false;
    updateFloatingControls();
    timerInterval = setInterval(() => {
        if (totalSeconds > 0) {
            totalSeconds--;
            updateDisplay();
        } else {
            clearInterval(timerInterval);
            isRunning = false;
            
           localStorage.removeItem('sessionCompleteShown'); 
            showSessionComplete();
        }
    }, 1000);
}

function pauseTimer() {
    if (isRunning) {
        clearInterval(timerInterval);
        isRunning = false;
        isPaused = true;
        saveTimerState();
        updateFloatingControls();
    }
}

function resumeTimer() {
    if (isPaused && totalSeconds > 0) {
        startTimer();
        updateFloatingControls();
    }
}

function showFloatingTimer() {
    if (floatingTimer) floatingTimer.classList.add('show');
}

function hideFloatingTimer() {
    if (floatingTimer) floatingTimer.classList.remove('show');
}

function showSessionComplete() {
    // Only show modal ONCE 
    if (localStorage.getItem('sessionCompleteShown') === 'true') return;
    clearInterval(timerInterval);
    isRunning = false;
    const sessionEndTime = new Date();
    const actualDurationInSeconds = sessionStartTime
        ? Math.max(0, Math.round((sessionEndTime - sessionStartTime) / 1000))
        : 0;
    if (completedSubject) completedSubject.textContent = currentSubject || "this subject";
    if (completedDuration) completedDuration.textContent = formatDuration(actualDurationInSeconds);
    if (sessionCompleteModal && !sessionCompleteModal.classList.contains('show')) {
        sessionCompleteModal.style.display = 'block';
        sessionCompleteModal.classList.add('show');
    }
    localStorage.setItem('sessionCompleteShown', 'true');
}

function extendSession(minutes) {
    const extraSeconds = minutes * 60;
    totalSeconds += extraSeconds;
    originalTotalSeconds += extraSeconds;
    if (sessionCompleteModal) {
        sessionCompleteModal.classList.remove('show');
        sessionCompleteModal.style.display = 'none';
    }
    localStorage.removeItem('sessionCompleteShown');
    updateDisplay();
    startTimer();
}

async function saveSessionToDatabase() {
    if (!sessionStartTime || !currentSubject) return;
    const sessionEndTime = new Date();
    const actualDurationInSeconds = Math.max(0, Math.round((sessionEndTime - sessionStartTime) / 1000));
    const sessionData = {
        Subject: currentSubject,
        StartTime: sessionStartTime.toISOString(),
        EndTime: sessionEndTime.toISOString(),
        Duration: `${Math.floor(actualDurationInSeconds / 3600)}:${Math.floor((actualDurationInSeconds % 3600) / 60)}:${actualDurationInSeconds % 60}`
        // UserId, Id set by server/controller
    };
    try {
        const response = await fetch('/api/study/save-session', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(sessionData)
        });
        // ... handle response
    } catch (error) {
        // ... handle error
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const startStudyBtn = document.getElementById('startStudyBtn');
    const hoursInput = document.getElementById('hours');
    const minutesInput = document.getElementById('minutes');
    const secondsInput = document.getElementById('seconds');
    const subjectInput = document.getElementById('subject');
    const durationBtns = document.querySelectorAll('.duration-btn');
    if (!startStudyBtn) return;
    durationBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            durationBtns.forEach(b => b.classList.remove('selected'));
            this.classList.add('selected');
            const minutes = parseInt(this.dataset.minutes);
            hoursInput.value = Math.floor(minutes / 60);
            minutesInput.value = minutes % 60;
            secondsInput.value = 0;
        });
    });
    startStudyBtn.addEventListener('click', function () {
        const subject = subjectInput.value;
        const hours = parseInt(hoursInput.value) || 0;
        const minutes = parseInt(minutesInput.value) || 0;
        const seconds = parseInt(secondsInput.value) || 0;
        if (!subject.trim()) {
            Swal.fire({
                icon: 'warning',
                title: 'Oops!',
                text: 'Please enter a subject',
                confirmButtonText: 'Okay',
                confirmButtonColor: '#3085d6'
            });
            return;
        }
        totalSeconds = (hours * 3600) + (minutes * 60) + seconds;
        originalTotalSeconds = totalSeconds;
        currentSubject = subject;
        if (totalSeconds <= 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Oops!',
                text: 'Please set a study duration',
                confirmButtonText: 'Okay',
                confirmButtonColor: '#3085d6'
            });
            return;
        }
        if (studySubject) studySubject.textContent = subject;
        if (floatingSubject) floatingSubject.textContent = subject;
        if (sessionDuration) sessionDuration.textContent = formatDuration(totalSeconds);
        updateDisplay();
        if (studyForm) studyForm.style.display = 'none';
        if (timerSection) timerSection.classList.add('show');
        showFloatingTimer();
        sessionStartTime = new Date();
        startTimer();
    });
});

if (pauseBtn) pauseBtn.addEventListener('click', pauseTimer);
if (resumeBtn) resumeBtn.addEventListener('click', resumeTimer);
if (floatingPauseBtn) floatingPauseBtn.addEventListener('click', pauseTimer);
if (floatingResumeBtn) floatingResumeBtn.addEventListener('click', resumeTimer);

if (stopBtn) {
    stopBtn.addEventListener('click', () => {
        Swal.fire({
            title: 'End session?',
            text: "Are you sure you want to stop this session?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, end it',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) endSession();
        });
    });
}
if (floatingStopBtn) {
    floatingStopBtn.addEventListener('click', () => {
        Swal.fire({
            title: 'End session?',
            text: "Are you sure you want to stop this session?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, end it',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) endSession();
        });
    });
}

// Save state on tab close
window.addEventListener('beforeunload', () => {
    if (isRunning || isPaused) saveTimerState();
});

document.addEventListener('visibilitychange', () => {
    if (document.hidden) saveTimerState();
});

document.addEventListener('DOMContentLoaded', function () {
    const finishNowBtn = document.getElementById('finishNowBtn');
    if (finishNowBtn) {
        finishNowBtn.addEventListener('click', async () => {
            await finishNow();
        });
    }
});

// endSession function (unchanged, but place here for completeness)
function endSession() {
    localStorage.removeItem('pausedSessionAlertShown');
    clearInterval(timerInterval);
    isRunning = false;
    isPaused = false;
    const sessionEndTime = new Date();
    const actualDurationInSeconds = sessionStartTime
        ? Math.max(0, Math.round((sessionEndTime - sessionStartTime) / 1000))
        : 0;
    if (completedSubject) completedSubject.textContent = currentSubject || "this subject";
    if (completedDuration) completedDuration.textContent = formatDuration(actualDurationInSeconds);
    if (sessionCompleteModal) {
        sessionCompleteModal.style.display = 'block';
        sessionCompleteModal.classList.add('show');
    }
    localStorage.setItem('sessionCompleteShown', 'true');
}
