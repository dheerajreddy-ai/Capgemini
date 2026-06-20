/**
 * Storyboard API calls — thin wrappers over the generic `api` client.
 */

import { api } from "./api";

export interface ShotOut {
  id: string;
  shot_number: number;
  shot_name: string | null;
  shot_type: string | null;
  camera_angle: string | null;
  camera_movement: string | null;
  lens: string | null;
  lighting: string | null;
  mood: string | null;
  duration: string | null;
  prompt: string | null;
  explanation: string | null;
  image_url: string | null;
}

export interface StoryboardOut {
  id: string;
  title: string | null;
  scene_description: string;
  scene_style: string | null;
  director_note: string | null;
  status: "pending" | "processing" | "completed" | "failed";
  shots: ShotOut[];
  created_at: string;
}

export interface StoryboardSummary {
  id: string;
  title: string | null;
  scene_style: string | null;
  status: "pending" | "processing" | "completed" | "failed";
  shot_count: number;
  created_at: string;
}

export interface GenerateRequest {
  scene_description: string;
  scene_style?: string;
  location_desc?: string | null;
  character_desc?: string | null;
}

export const storyboardApi = {
  generate: (body: GenerateRequest) =>
    api.post<StoryboardOut>("/storyboard/generate", body),

  history: (limit = 20, offset = 0) =>
    api.get<StoryboardSummary[]>(`/storyboard/history?limit=${limit}&offset=${offset}`),

  getOne: (id: string) =>
    api.get<StoryboardOut>(`/storyboard/${id}`),

  delete: (id: string) =>
    api.del<{ deleted: boolean }>(`/storyboard/${id}`),
};
