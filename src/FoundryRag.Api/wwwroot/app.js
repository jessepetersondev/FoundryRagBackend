const healthBadge = document.querySelector("#healthBadge");
const healthButton = document.querySelector("#healthButton");
const ingestButton = document.querySelector("#ingestButton");
const ingestStatus = document.querySelector("#ingestStatus");
const askForm = document.querySelector("#askForm");
const askButton = document.querySelector("#askButton");
const questionInput = document.querySelector("#questionInput");
const topKInput = document.querySelector("#topKInput");
const requestState = document.querySelector("#requestState");
const answerOutput = document.querySelector("#answerOutput");
const retrievalMeta = document.querySelector("#retrievalMeta");
const sourcesBody = document.querySelector("#sourcesBody");
const sampleButtons = document.querySelectorAll("[data-question]");

function setBadge(element, text, kind) {
  element.textContent = text;
  element.className = `badge badge-${kind}`;
}

function setBusy(isBusy) {
  askButton.disabled = isBusy;
  questionInput.disabled = isBusy;
  topKInput.disabled = isBusy;
}

async function parseJsonResponse(response) {
  const text = await response.text();
  const body = text ? JSON.parse(text) : {};

  if (!response.ok) {
    const message = body.message || body.error || `Request failed with status ${response.status}`;
    throw new Error(message);
  }

  return body;
}

async function refreshHealth() {
  setBadge(healthBadge, "Checking", "muted");
  healthButton.disabled = true;

  try {
    const response = await fetch("/api/health");
    await parseJsonResponse(response);
    setBadge(healthBadge, "Connected", "ok");
  } catch (error) {
    setBadge(healthBadge, "Offline", "error");
  } finally {
    healthButton.disabled = false;
  }
}

async function ingestSeedData() {
  ingestButton.disabled = true;
  ingestStatus.textContent = "Ingesting seed data";

  try {
    const response = await fetch("/api/dev/ingest", { method: "POST" });
    const result = await parseJsonResponse(response);
    ingestStatus.textContent = `Uploaded ${result.documentsUploaded} of ${result.documentsRead} documents to ${result.indexName}`;
  } catch (error) {
    ingestStatus.textContent = error.message;
  } finally {
    ingestButton.disabled = false;
  }
}

function renderSources(sources) {
  sourcesBody.replaceChildren();

  if (!sources || sources.length === 0) {
    const row = document.createElement("tr");
    const cell = document.createElement("td");
    cell.colSpan = 4;
    cell.className = "empty-cell";
    cell.textContent = "No sources";
    row.append(cell);
    sourcesBody.append(row);
    return;
  }

  for (const source of sources) {
    const row = document.createElement("tr");
    const id = document.createElement("td");
    const title = document.createElement("td");
    const category = document.createElement("td");
    const score = document.createElement("td");

    id.className = "source-id";
    id.textContent = source.id;
    title.textContent = source.title;
    category.textContent = source.category;
    score.className = "score";
    score.textContent = source.score === null || source.score === undefined
      ? "-"
      : Number(source.score).toFixed(4);

    row.append(id, title, category, score);
    sourcesBody.append(row);
  }
}

async function askQuestion(event) {
  event.preventDefault();

  const question = questionInput.value.trim();
  const topK = Number.parseInt(topKInput.value, 10);

  if (!question) {
    questionInput.focus();
    return;
  }

  setBusy(true);
  setBadge(requestState, "Running", "warn");
  retrievalMeta.textContent = "Retrieving context";
  answerOutput.textContent = "";
  renderSources([]);

  try {
    const response = await fetch("/api/ask", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        question,
        topK: Number.isFinite(topK) ? topK : null
      })
    });

    const result = await parseJsonResponse(response);
    answerOutput.textContent = result.answer;
    retrievalMeta.textContent = `${result.retrieval.documentsReturned} documents returned for topK ${result.retrieval.topKRequested}`;
    renderSources(result.sources);
    setBadge(requestState, "Complete", "ok");
  } catch (error) {
    answerOutput.textContent = error.message;
    retrievalMeta.textContent = "Request failed";
    setBadge(requestState, "Error", "error");
  } finally {
    setBusy(false);
  }
}

healthButton.addEventListener("click", refreshHealth);
ingestButton.addEventListener("click", ingestSeedData);
askForm.addEventListener("submit", askQuestion);

for (const button of sampleButtons) {
  button.addEventListener("click", () => {
    questionInput.value = button.dataset.question;
    questionInput.focus();
  });
}

refreshHealth();
